
import { useEffect, useRef, useState } from "react";
import type { ChangeEvent, FormEvent } from "react";

import { api } from "./api";
import type { Operation, Summary } from "./api";

// ==================================================
// Types
// ==================================================

type Mode = "deposit" | "withdraw" | "partner" | "valuation";

// ==================================================
// Constants
// ==================================================

const persianDigits = "۰۱۲۳۴۵۶۷۸۹";
const arabicDigits = "٠١٢٣٤٥٦٧٨٩";

const numberFormatter = new Intl.NumberFormat("fa-IR");

const operationTitles: Record<string, string> = {
    Deposit: "واریز",
    Withdrawal: "برداشت",
    ValuationUpdate: "تغییر ارزش"
};

const modes: { value: Mode; title: string }[] = [
    { value: "deposit", title: "واریز" },
    { value: "withdraw", title: "برداشت" },
    { value: "partner", title: "شریک جدید" },
    { value: "valuation", title: "ارزش‌گذاری" }
];

// ==================================================
// Money Helpers
// ==================================================

function normalizeDigits(value: string): string {
    return value
        .replace(/[۰-۹]/g, digit =>
            String(persianDigits.indexOf(digit))
        )
        .replace(/[٠-٩]/g, digit =>
            String(arabicDigits.indexOf(digit))
        );
}

function extractDigits(value: string): string {
    return normalizeDigits(value).replace(/\D/g, "");
}

function formatMoneyInput(value: string): string {
    const digits = extractDigits(value);

    if (!digits)
        return "";

    return numberFormatter.format(Number(digits));
}

function parseToman(value: string): number {
    const digits = extractDigits(value);

    if (!digits)
        return NaN;

    const toman = Number(digits);
    const rial = toman * 10;

    if (
        !Number.isSafeInteger(toman) ||
        !Number.isSafeInteger(rial) ||
        toman < 0
    ) {
        return NaN;
    }

    return rial;
}

function formatMoney(rial: number): string {
    return numberFormatter.format(Math.round(rial / 10)) + " تومان";
}

// ==================================================
// Reusable Components
// ==================================================

type MoneyInputProps = {
    label: string;
    value: string;
    onChange: (value: string) => void;
    placeholder?: string;
    required?: boolean;
};

function MoneyInput({
    label,
    value,
    onChange,
    placeholder,
    required = true
}: MoneyInputProps) {
    const inputRef = useRef<HTMLInputElement>(null);

    function handleChange(event: ChangeEvent<HTMLInputElement>) {
        
        console.log("MoneyInput is working");
        const input = event.target;
        const oldValue = input.value;
        console.log("Before:", oldValue);        

        const oldCursor = input.selectionStart ?? oldValue.length;
        
        // تعداد ارقام قبل از محل مکان‌نما
        const digitsBeforeCursor = extractDigits(
            oldValue.slice(0, oldCursor)
        ).length;

        const formattedValue = formatMoneyInput(oldValue);
        console.log("After:", formattedValue);
        
        onChange(formattedValue);

        // بازگرداندن مکان‌نما پس از فرمت‌شدن
        requestAnimationFrame(() => {
            const element = inputRef.current;

            if (!element)
                return;

            let newCursor = 0;
            let digitCount = 0;

            while (
                newCursor < formattedValue.length &&
                digitCount < digitsBeforeCursor
            ) {
                if (/\d/.test(normalizeDigits(formattedValue[newCursor]))) {
                    digitCount++;
                }

                newCursor++;
            }

            // عبور از جداکننده‌ای که بعد از رقم قرار گرفته است
            while (
                newCursor < formattedValue.length &&
                !/\d/.test(normalizeDigits(formattedValue[newCursor]))
            ) {
                newCursor++;
            }

            element.setSelectionRange(newCursor, newCursor);
        });
    }

    return (
        <label>
            {label}

            <input
                ref={inputRef}
                type="text"
                inputMode="numeric"
                autoComplete="off"
                required={required}
                value={value}
                placeholder={placeholder}
                onChange={handleChange}
            />
        </label>
    );
}

// ==================================================
// Main App
// ==================================================

export default function App() {

    // --------------------------------------------------
    // State
    // --------------------------------------------------

    const [summary, setSummary] = useState<Summary | null>(null);
    const [ops, setOps] = useState<Operation[]>([]);

    const [mode, setMode] = useState<Mode>("deposit");

    const [error, setError] = useState("");
    const [busy, setBusy] = useState(false);

    // Partner
    const [code, setCode] = useState("");
    const [first, setFirst] = useState("");
    const [last, setLast] = useState("");
    const [partner, setPartner] = useState("");

    // Financial
    const [cash, setCash] = useState("");
    const [stock, setStock] = useState("");
    const [amount, setAmount] = useState("");
    const [note, setNote] = useState("");

    // --------------------------------------------------
    // Load Data
    // --------------------------------------------------

    async function refresh() {
        const [portfolioSummary, operations] = await Promise.all([
            api.summary(),
            api.operations()
        ]);

        setSummary(portfolioSummary);
        setOps(operations);

        setCash(
            formatMoneyInput(
                String(portfolioSummary.cashValueRial / 10)
            )
        );

        setStock(
            formatMoneyInput(
                String(portfolioSummary.stockValueRial / 10)
            )
        );
    }

    useEffect(() => {
        refresh().catch(error => {
            setError(
                error instanceof Error
                    ? error.message
                    : "خطا در دریافت اطلاعات"
            );
        });
    }, []);

    // --------------------------------------------------
    // Submit
    // --------------------------------------------------

    async function submit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();

        if (busy)
            return;

        setBusy(true);
        setError("");

        try {
            // Create Partner
            if (mode === "partner") {

                const partnerCode = Number(code);

                if (
                    !Number.isSafeInteger(partnerCode) ||
                    partnerCode <= 0
                ) {
                    throw new Error("کد شریک معتبر نیست.");
                }

                await api.partner({
                    code: partnerCode,
                    firstName: first.trim(),
                    lastName: last.trim()
                });
            }

            // Financial Operations
            else {

                const cashRial = parseToman(cash);
                const stockRial = parseToman(stock);

                if (
                    !Number.isSafeInteger(cashRial) ||
                    !Number.isSafeInteger(stockRial)
                ) {
                    throw new Error(
                        "وجه نقد و ارزش سهام را صحیح وارد کنید."
                    );
                }

                // Valuation
                if (mode === "valuation") {

                    await api.valuation({
                        cashValueRial: cashRial,
                        stockValueRial: stockRial,
                        note
                    });
                }

                // Deposit / Withdrawal
                else {

                    const amountRial = parseToman(amount);
                    const partnerCode = Number(partner);

                    if (
                        !Number.isSafeInteger(amountRial) ||
                        amountRial <= 0
                    ) {
                        throw new Error(
                            "مبلغ عملیات باید بیشتر از صفر باشد."
                        );
                    }

                    if (
                        !Number.isSafeInteger(partnerCode) ||
                        partnerCode <= 0
                    ) {
                        throw new Error("لطفاً شریک را انتخاب کنید.");
                    }

                    const body = {
                        partnerCode,
                        currentCashValueRial: cashRial,
                        currentStockValueRial: stockRial,
                        amountRial,
                        note
                    };

                    if (mode === "deposit") {
                        await api.deposit(body);
                    } else {
                        await api.withdraw(body);
                    }
                }
            }

            setAmount("");
            setNote("");

            await refresh();

        } catch (error) {

            setError(
                error instanceof Error
                    ? error.message
                    : "خطای نامشخص"
            );

        } finally {
            setBusy(false);
        }
    }

    // --------------------------------------------------
    // Form Sections
    // --------------------------------------------------

    function renderCashFields() {
        return (
            <>
                <MoneyInput
                    label="وجه نقد فعلی (تومان)"
                    value={cash}
                    onChange={setCash}
                    placeholder="مثلاً ۳۰٬۰۰۰٬۰۰۰"
                />

                <MoneyInput
                    label="ارزش فعلی سهام (تومان)"
                    value={stock}
                    onChange={setStock}
                    placeholder="مثلاً ۷۰٬۰۰۰٬۰۰۰"
                />

                <aside>
                    ارزش کل پرتفو از جمع وجه نقد و ارزش سهام
                    محاسبه می‌شود.
                </aside>
            </>
        );
    }

    function renderPartnerForm() {
        return (
            <>
                <label>
                    کد شریک

                    <input
                        required
                        type="number"
                        min="1"
                        value={code}
                        onChange={event => setCode(event.target.value)}
                    />
                </label>

                <div className="two">
                    <label>
                        نام

                        <input
                            required
                            value={first}
                            onChange={event => setFirst(event.target.value)}
                        />
                    </label>

                    <label>
                        نام خانوادگی

                        <input
                            required
                            value={last}
                            onChange={event => setLast(event.target.value)}
                        />
                    </label>
                </div>

                <aside>
                    ثبت شریک به‌تنهایی مالکیت ایجاد نمی‌کند؛
                    با اولین واریز سهم می‌گیرد.
                </aside>
            </>
        );
    }

    function renderCapitalFields() {
        return (
            <>
                {renderCashFields()}

                {mode !== "valuation" && (
                    <>
                        <label>
                            شریک

                            <select
                                required
                                value={partner}
                                onChange={event =>
                                    setPartner(event.target.value)
                                }
                            >
                                <option value="">
                                    انتخاب شریک
                                </option>

                                {summary?.partners.map(item => (
                                    <option
                                        key={item.id}
                                        value={item.code}
                                    >
                                        {item.code} - {item.fullName}
                                    </option>
                                ))}
                            </select>
                        </label>

                        <MoneyInput
                            label={`مبلغ ${
                                mode === "deposit" ? "واریز" : "برداشت"
                            } (تومان)`}
                            value={amount}
                            onChange={setAmount}
                            placeholder="مبلغ را وارد کنید"
                        />
                    </>
                )}

                {mode === "deposit" && (
                    <aside>
                        مبلغ تخصیص به شرکای قبلی = مبلغ واریزی
                        × (ارزش سهام ÷ کل پرتفو) × ۱٪.
                        وجه نقد در محاسبه مالکیت دخالت ندارد.
                    </aside>
                )}

                {mode === "withdraw" && (
                    <aside>
                        برداشت فقط از وجه نقد انجام می‌شود.
                        اگر وجه نقد کافی نباشد، ابتدا باید سهام فروخته شود.
                        سپس سهم ریالی و نسبت مالکیت شرکا محاسبه می‌شود.
                    </aside>
                )}

                {mode === "valuation" && (
                    <aside>
                        با تغییر ارزش وجه نقد یا سهام، ارزش سهم همه شرکا
                        متناسب با نسبت قبلی آن‌ها به‌روزرسانی می‌شود.
                    </aside>
                )}
            </>
        );
    }

    // --------------------------------------------------
    // Render
    // --------------------------------------------------

    return (
        <main className="wrap">

            {/* Header */}

            <header>
                <div>
                    <small>PORTFOLIO MANAGER</small>
                    <h1>مدیریت پرتفو</h1>
                </div>

                <span>
                    مدیریت سهم شرکا بر اساس ارزش کل پرتفو
                </span>
            </header>

            {/* Error */}

            {error && (
                <div className="error">
                    {error}

                    <button
                        type="button"
                        onClick={() => setError("")}
                    >
                        ×
                    </button>
                </div>
            )}

            {/* Portfolio Statistics */}

            <section className="stats">

                <article className="hero">
                    <label>ارزش کل پرتفو</label>

                    <strong>
                        {summary
                            ? formatMoney(summary.totalValueRial)
                            : "—"}
                    </strong>

                    <small>وجه نقد + ارزش سهام</small>
                </article>

                <article>
                    <label>وجه نقد</label>

                    <strong>
                        {summary
                            ? formatMoney(summary.cashValueRial)
                            : "—"}
                    </strong>
                </article>

                <article>
                    <label>ارزش سهام</label>

                    <strong>
                        {summary
                            ? formatMoney(summary.stockValueRial)
                            : "—"}
                    </strong>
                </article>

                <article>
                    <label>تعداد شرکا</label>

                    <strong>
                        {summary?.partners.length ?? "—"}
                    </strong>
                </article>

            </section>

            {/* Main Columns */}

            <section className="columns">

                {/* Operation Form */}

                <article className="panel">

                    <h2>ثبت عملیات</h2>
                    <p>مبالغ را به تومان وارد کنید.</p>

                    <nav>
                        {modes.map(item => (
                            <button
                                key={item.value}
                                type="button"
                                className={
                                    mode === item.value ? "selected" : ""
                                }
                                onClick={() => setMode(item.value)}
                            >
                                {item.title}
                            </button>
                        ))}
                    </nav>

                    <form onSubmit={submit}>

                        {mode === "partner"
                            ? renderPartnerForm()
                            : renderCapitalFields()}

                        <label>
                            توضیحات (اختیاری)

                            <input
                                value={note}
                                onChange={event =>
                                    setNote(event.target.value)
                                }
                            />
                        </label>

                        <button
                            className="submit"
                            type="submit"
                            disabled={busy}
                        >
                            {busy ? "در حال ثبت..." : "ثبت عملیات"}
                        </button>

                    </form>

                </article>

                {/* Partners */}

                <article className="panel">

                    <h2>شرکا و درصد مالکیت</h2>

                    <p>
                        درصد فقط برای نمایش از نسبت مالکیت × ۱۰۰
                        محاسبه می‌شود.
                    </p>

                    <div className="scroll">

                        <table>
                            <thead>
                                <tr>
                                    <th>شریک</th>
                                    <th>ارزش سهم</th>
                                    <th>درصد</th>
                                </tr>
                            </thead>

                            <tbody>

                                {summary?.partners.map(item => (
                                    <tr key={item.id}>

                                        <td>
                                            {item.fullName}
                                            <small>کد {item.code}</small>
                                        </td>

                                        <td>
                                            {formatMoney(item.shareValueRial)}
                                        </td>

                                        <td>
                                            {(item.ownershipRatio * 100).toFixed(4)}٪

                                            <div className="bar">
                                                <i
                                                    style={{
                                                        width: `${Math.min(
                                                            100,
                                                            item.ownershipRatio * 100
                                                        )}%`
                                                    }}
                                                />
                                            </div>
                                        </td>

                                    </tr>
                                ))}

                                {!summary?.partners.length && (
                                    <tr>
                                        <td colSpan={3}>
                                            هنوز شریکی ثبت نشده است.
                                        </td>
                                    </tr>
                                )}

                            </tbody>
                        </table>

                    </div>

                    <aside>
                        جمع نسبت مالکیت:{" "}
                        {summary
                            ? (summary.totalOwnershipRatio * 100).toFixed(4)
                            : "—"}
                        ٪
                    </aside>

                </article>

            </section>

            {/* Operation History */}

            <article className="panel history">

                <h2>تاریخچه عملیات</h2>

                <div className="scroll">

                    <table>
                        <thead>
                            <tr>
                                <th>نوع</th>
                                <th>شریک</th>
                                <th>مبلغ</th>
                                <th>پرتفو قبل</th>
                                <th>پرتفو بعد</th>
                                <th>نقد قبل</th>
                                <th>نقد بعد</th>
                                <th>تاریخ</th>
                            </tr>
                        </thead>

                        <tbody>

                            {ops.map(operation => (
                                <tr key={operation.id}>

                                    <td>
                                        {operationTitles[operation.type]
                                            ?? operation.type}
                                    </td>

                                    <td>
                                        {operation.partnerName ?? "—"}
                                    </td>

                                    <td>
                                        {formatMoney(operation.amountRial)}
                                    </td>

                                    <td>
                                        {formatMoney(operation.beforeRial)}
                                    </td>

                                    <td>
                                        {formatMoney(operation.afterRial)}
                                    </td>

                                    <td>
                                        {formatMoney(operation.cashBeforeRial)}
                                    </td>

                                    <td>
                                        {formatMoney(operation.cashAfterRial)}
                                    </td>

                                    <td>
                                        {new Date(
                                            operation.createdAtUtc
                                        ).toLocaleString("fa-IR")}
                                    </td>

                                </tr>
                            ))}

                            {!ops.length && (
                                <tr>
                                    <td colSpan={8}>
                                        عملیاتی ثبت نشده است.
                                    </td>
                                </tr>
                            )}

                        </tbody>
                    </table>

                </div>

            </article>

            {/* Footer */}

            <footer>
                مبالغ دیتابیس: ریال · مبالغ ورودی: تومان ·
                مالکیت بر اساس کل پرتفو · وجه نقد و سهام جداگانه ثبت می‌شوند
            </footer>

        </main>
    );
}
