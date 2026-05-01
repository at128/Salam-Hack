import { getValidAccessToken, getApiErrorMessage, unwrapApiResponse } from "./auth";

export interface ProfitabilityReportDto {
    period: { fromUtc: string; toUtc: string };
    summary: { totalRevenue: number; totalExpenses: number; totalProfit: number; overallMarginPercent: number };
    monthlyTrend: { year: number; month: number; revenue: number; expenses: number; profit: number }[];
    byService: { entityId: string; name: string; type: string; revenue: number; cost: number; profit: number; marginPercent: number }[];
    byCustomer: any[];
    byProject: any[];
    topPerformers: any[];
    lowestPerformers: any[];
    insight: string;
}

export interface CashFlowForecastDto {
    period: { fromUtc: string; toUtc: string };
    openingBalance: { balanceUtc: string; amount: number };
    currentBalance: number;
    currentMonthInflows: number;
    currentMonthOutflows: number;
    currentMonthNetFlow: number;
    monthlyTrend: { year: number; month: number; inflows: number; outflows: number; netFlow: number; balance: number }[];
    projection: {
        fromUtc: string;
        toUtc: string;
        expectedInflows: number;
        expectedOutflows: number;
        expectedNetFlow: number;
        forecastBalance: number;
    };
    pendingInvoices: {
        invoiceId: string;
        invoiceNumber: string;
        customerId: string;
        customerName: string;
        remainingAmount: number;
        dueDate: string;
        isOverdue: boolean;
        currency: string;
    }[];
    recurringExpenses: {
        expenseId: string;
        description: string;
        amount: number;
        recurrenceInterval: string;
        monthlyEquivalentAmount: number;
        expenseDate: string;
        recurrenceEndDate?: string | null;
        currency: string;
    }[];
    clientDelayScenario: {
        customerId: string;
        customerName: string;
        delayedAmount: number;
        forecastBalanceAfterDelay: number;
        wouldGoNegative: boolean;
    } | null;
}

function getApiBaseUrl() {
    return (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
}

export async function fetchProfitabilityReport(fromUtc?: string, toUtc?: string): Promise<ProfitabilityReportDto> {
    const token = await getValidAccessToken();
    if (!token) throw new Error("Missing access token.");

    const params = new URLSearchParams();
    if (fromUtc) params.append("fromUtc", fromUtc);
    if (toUtc) params.append("toUtc", toUtc);

    const query = params.toString();
    const url = `${getApiBaseUrl()}/api/v1/Reports/profitability${query ? `?${query}` : ""}`;

    const response = await fetch(url, {
        method: "GET",
        headers: {
            Accept: "application/json",
            Authorization: `Bearer ${token}`,
        },
    });

    const payload = await response.json().catch(() => null);

    if (!response.ok) throw new Error(getApiErrorMessage(payload, "Unable to load profitability report."));

    return unwrapApiResponse<ProfitabilityReportDto>(payload);
}

export async function fetchCashFlowForecast(
    asOfUtc?: string,
    openingBalance: number = 0,
    openingBalanceDateUtc?: string
): Promise<CashFlowForecastDto> {
    const token = await getValidAccessToken();
    if (!token) throw new Error("Missing access token.");

    const params = new URLSearchParams();
    if (asOfUtc) params.append("asOfUtc", asOfUtc);
    if (openingBalance > 0) params.append("openingBalance", openingBalance.toString());
    if (openingBalanceDateUtc) params.append("openingBalanceDateUtc", openingBalanceDateUtc);

    const query = params.toString();
    const url = `${getApiBaseUrl()}/api/v1/Reports/cashflow${query ? `?${query}` : ""}`;

    const response = await fetch(url, {
        method: "GET",
        headers: {
            Accept: "application/json",
            Authorization: `Bearer ${token}`,
        },
    });

    const payload = await response.json().catch(() => null);

    if (!response.ok) throw new Error(getApiErrorMessage(payload, "Unable to load cash flow forecast."));

    return unwrapApiResponse<CashFlowForecastDto>(payload);
}
