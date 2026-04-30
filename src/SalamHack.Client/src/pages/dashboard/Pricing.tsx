import { useEffect, useMemo, useRef, useState } from "react";
import { CalendarDays, FileText, Loader2, Sparkles, TrendingUp, Wand2 } from "lucide-react";
import { PageHeader } from "@/components/dashboard/DashboardLayout";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { getApiErrorMessage, getValidAccessToken, unwrapApiResponse } from "@/lib/auth";

type ComplexityLevel = "Simple" | "Medium" | "Complex";
type PricingPlanType = "Economy" | "Recommended" | "Premium";

type PaginatedList<T> = {
  items: T[];
};

type CustomerOption = {
  id: string;
  customerName: string;
};

type ServiceOption = {
  id: string;
  serviceName: string;
  defaultHourlyRate?: number;
  defaultRevisions?: number;
  isActive: boolean;
};

type PricingPlan = {
  planType: PricingPlanType | "economy" | "recommended" | "premium" | number;
  name: string;
  price: number;
  marginPercent: number;
  advanceAmount: number;
  isViable: boolean;
};

type PricingQuote = {
  serviceId: string;
  serviceName: string;
  complexity: ComplexityLevel | number;
  estimatedHours: number;
  adjustedHours: number;
  realCost: number;
  minAcceptablePrice: number;
  targetMarginPercent: number;
  naivePrice: number;
  naiveMarginPercent: number;
  history: {
    completedProjectCount: number;
    averageEstimatedHours: number;
    averageActualHours: number;
    averageMarginPercent: number;
    hoursOverrunFactor: number;
    costOverrunFactor: number;
    averageExtraExpenses: number;
    hasHistory: boolean;
  };
  recentProjects: {
    projectId: string;
    projectName: string;
    estimatedHours: number;
    actualHours: number;
    suggestedPrice: number;
    actualCost: number;
    extraExpenses: number;
    actualMarginPercent: number;
    completedAt: string;
  }[];
  plans: PricingPlan[];
  insights: {
    severity: "Info" | "Success" | "Warning" | "Critical" | "info" | "success" | "warning" | "critical" | number;
    message: string;
  }[];
  adjustments: {
    serviceHourlyRate: number;
    complexityMultiplier: number;
    historicalHoursFactor: number;
    appliedCostFactor: number;
    includedRevisions: number;
    requestedRevisions: number;
    extraRevisions: number;
    isUrgent: boolean;
    revisionMultiplier: number;
    urgencyMultiplier: number;
    confidenceMultiplier: number;
  };
};

type QuoteForm = {
  serviceId: string;
  estimatedHours: string;
  complexity: ComplexityLevel;
  recentProjectCount: string;
  toolCost: string;
  revision: string;
  isUrgent: boolean;
};

type CreateForm = {
  customerId: string;
  projectName: string;
  selectedPlan: PricingPlanType;
  startDate: string;
  endDate: string;
  invoiceNumber: string;
  issueDate: string;
  dueDate: string;
  currency: string;
  notes: string;
};

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const PRICING_API_URL = `${API_BASE_URL}/api/v1/pricing`;
const SERVICES_API_URL = `${API_BASE_URL}/api/v1/services`;
const CUSTOMERS_API_URL = `${API_BASE_URL}/api/v1/customers`;

const EMPTY_QUOTE_FORM: QuoteForm = {
  serviceId: "",
  estimatedHours: "",
  complexity: "Medium",
  recentProjectCount: "5",
  toolCost: "0",
  revision: "",
  isUrgent: false,
};

const today = new Date().toISOString().slice(0, 10);
const EMPTY_CREATE_FORM: CreateForm = {
  customerId: "",
  projectName: "",
  selectedPlan: "Recommended",
  startDate: today,
  endDate: today,
  invoiceNumber: "",
  issueDate: today,
  dueDate: today,
  currency: "SAR",
  notes: "",
};

function buildQuery(params: Record<string, string | number | boolean | undefined>) {
  const searchParams = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === "") continue;
    searchParams.set(key, String(value));
  }

  const query = searchParams.toString();
  return query ? `?${query}` : "";
}

async function apiRequest<T>(url: string, init?: RequestInit): Promise<T> {
  const token = await getValidAccessToken();
  if (!token) throw new Error("Missing access token.");

  const response = await fetch(url, {
    ...init,
    headers: {
      Accept: "application/json",
      Authorization: `Bearer ${token}`,
      ...(init?.body ? { "Content-Type": "application/json" } : {}),
      ...init?.headers,
    },
  });

  const payload = await response.json().catch(() => null);

  if (!response.ok) {
    throw payload ?? new Error(getApiErrorMessage(payload, "تعذر تنفيذ الطلب."));
  }

  return unwrapApiResponse<T>(payload);
}

function dateInputToIso(value: string) {
  return new Date(`${value}T00:00:00.000Z`).toISOString();
}

function normalizePlanType(planType: PricingPlan["planType"]): PricingPlanType {
  const normalized = typeof planType === "string" ? planType.toLowerCase() : planType;
  if (normalized === 0 || normalized === "economy") return "Economy";
  if (normalized === 2 || normalized === "premium") return "Premium";
  return "Recommended";
}

function formatCurrency(value: number | undefined, currency = "SAR") {
  return new Intl.NumberFormat("ar", {
    style: "currency",
    currency,
    maximumFractionDigits: 0,
  }).format(value ?? 0);
}

function formatNumber(value: number | undefined, digits = 1) {
  return new Intl.NumberFormat("ar", {
    maximumFractionDigits: digits,
  }).format(value ?? 0);
}

function planLabel(planType: PricingPlan["planType"]) {
  switch (normalizePlanType(planType)) {
    case "Economy":
      return "اقتصادي";
    case "Premium":
      return "مميز";
    default:
      return "موصى به";
  }
}

function complexityLabel(complexity: ComplexityLevel) {
  switch (complexity) {
    case "Simple":
      return "بسيط";
    case "Complex":
      return "معقد";
    default:
      return "متوسط";
  }
}

function insightClass(severity: PricingQuote["insights"][number]["severity"]) {
  const normalized = typeof severity === "number" ? severity : severity.toLowerCase();
  if (normalized === "critical" || normalized === 3) return "border-danger/30 bg-danger-soft text-danger";
  if (normalized === "warning" || normalized === 2) return "border-warning/30 bg-warning-soft text-warning";
  if (normalized === "success" || normalized === 1) return "border-success/30 bg-success-soft text-success";
  return "border-border bg-muted/50 text-muted-foreground";
}

export default function PricingPage() {
  const [services, setServices] = useState<ServiceOption[]>([]);
  const [customers, setCustomers] = useState<CustomerOption[]>([]);
  const [quoteForm, setQuoteForm] = useState<QuoteForm>(EMPTY_QUOTE_FORM);
  const [createForm, setCreateForm] = useState<CreateForm>(EMPTY_CREATE_FORM);
  const [quote, setQuote] = useState<PricingQuote | null>(null);
  const [isLoadingLookups, setIsLoadingLookups] = useState(true);
  const [isCalculating, setIsCalculating] = useState(false);
  const [isCreatingProject, setIsCreatingProject] = useState(false);
  const [isCreatingInvoice, setIsCreatingInvoice] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const resultsRef = useRef<HTMLElement>(null);

  useEffect(() => {
    if (quote && resultsRef.current) {
      setTimeout(() => {
        resultsRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
      }, 100);
    }
  }, [quote]);

  const selectedPlan = useMemo(
    () => quote?.plans.find((plan) => normalizePlanType(plan.planType) === createForm.selectedPlan),
    [quote, createForm.selectedPlan],
  );

  const recommendedPlan = useMemo(
    () => quote?.plans.find((plan) => normalizePlanType(plan.planType) === "Recommended"),
    [quote],
  );

  useEffect(() => {
    const loadLookups = async () => {
      setIsLoadingLookups(true);
      setError("");

      try {
        const [serviceResult, customerResult] = await Promise.all([
          apiRequest<PaginatedList<ServiceOption>>(`${SERVICES_API_URL}?includeInactive=false&pageSize=100`),
          apiRequest<PaginatedList<CustomerOption>>(`${CUSTOMERS_API_URL}?pageSize=100`),
        ]);
        const activeServices = serviceResult.items ?? [];
        setServices(activeServices);
        setCustomers(customerResult.items ?? []);
        if (activeServices[0]) {
          setQuoteForm((prev) => ({
            ...prev,
            serviceId: prev.serviceId || activeServices[0].id,
            revision: prev.revision || String(activeServices[0].defaultRevisions ?? 0),
          }));
        }
      } catch (err) {
        setError(getApiErrorMessage(err, "تعذر تحميل الخدمات أو العملاء."));
      } finally {
        setIsLoadingLookups(false);
      }
    };

    void loadLookups();
  }, []);

  const setQuoteField = (field: keyof QuoteForm) => (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = field === "isUrgent" ? event.target.checked : event.target.value;
    setQuoteForm((prev) => ({ ...prev, [field]: value }));
    setError("");
    setMessage("");
  };

  const calculateQuote = async (event?: React.FormEvent) => {
    event?.preventDefault();
    setIsCalculating(true);
    setError("");
    setMessage("");

    try {
      const query = buildQuery({
        serviceId: quoteForm.serviceId,
        estimatedHours: Number(quoteForm.estimatedHours),
        complexity: quoteForm.complexity,
        recentProjectCount: Number(quoteForm.recentProjectCount || 5),
        toolCost: Number(quoteForm.toolCost || 0),
        revision: quoteForm.revision === "" ? undefined : Number(quoteForm.revision),
        isUrgent: quoteForm.isUrgent,
      });

      const result = await apiRequest<PricingQuote>(`${PRICING_API_URL}/quote${query}`);
      setQuote(result);
      setCreateForm((prev) => ({
        ...prev,
        projectName: prev.projectName || `${result.serviceName} - عرض سعر`,
        selectedPlan: "Recommended",
      }));
    } catch (err) {
      setQuote(null);
      setError(getApiErrorMessage(err, "تعذر حساب السعر المقترح."));
      window.scrollTo({ top: 0, behavior: "smooth" });
    } finally {
      setIsCalculating(false);
    }
  };

  const createPayload = () => {
    if (!quote) throw new Error("No quote selected.");

    return {
      customerId: createForm.customerId,
      serviceId: quote.serviceId,
      projectName: createForm.projectName.trim(),
      estimatedHours: Number(quoteForm.estimatedHours),
      complexity: quoteForm.complexity,
      selectedPlan: createForm.selectedPlan,
      toolCost: Number(quoteForm.toolCost || 0),
      revision: Number(quoteForm.revision || quote.adjustments.requestedRevisions || 0),
      isUrgent: quoteForm.isUrgent,
      startDate: dateInputToIso(createForm.startDate),
      endDate: dateInputToIso(createForm.endDate),
    };
  };

  const handleCreateProject = async () => {
    setIsCreatingProject(true);
    setError("");
    setMessage("");

    try {
      await apiRequest(`${PRICING_API_URL}/projects`, {
        method: "POST",
        body: JSON.stringify(createPayload()),
      });
      setMessage("تم إنشاء المشروع من عرض السعر بنجاح.");
      window.scrollTo({ top: 0, behavior: "smooth" });
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر إنشاء المشروع من عرض السعر."));
      window.scrollTo({ top: 0, behavior: "smooth" });
    } finally {
      setIsCreatingProject(false);
    }
  };

  const handleCreateInvoice = async () => {
    setIsCreatingInvoice(true);
    setError("");
    setMessage("");

    try {
      await apiRequest(`${PRICING_API_URL}/invoices`, {
        method: "POST",
        body: JSON.stringify({
          ...createPayload(),
          invoiceNumber: createForm.invoiceNumber.trim(),
          issueDate: dateInputToIso(createForm.issueDate),
          dueDate: dateInputToIso(createForm.dueDate),
          currency: createForm.currency.trim() || "SAR",
          notes: createForm.notes.trim() || null,
        }),
      });
      setMessage("تم إنشاء الفاتورة من عرض السعر بنجاح.");
      window.scrollTo({ top: 0, behavior: "smooth" });
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر إنشاء الفاتورة من عرض السعر."));
      window.scrollTo({ top: 0, behavior: "smooth" });
    } finally {
      setIsCreatingInvoice(false);
    }
  };

  return (
    <>
      <PageHeader title="التسعير الذكي" desc="احسب عرض سعر مبني على الخدمة، الساعات، التعقيد، تاريخ المشاريع، والتكاليف الإضافية." />

      {(message || error) && (
        <div
          className={`rounded-xl border p-3 text-sm ${
            error ? "border-danger/30 bg-danger-soft text-danger" : "border-success/30 bg-success-soft text-success"
          }`}
        >
          {error || message}
        </div>
      )}

      <section className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_360px]">
        <form onSubmit={calculateQuote} className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
          <div className="mb-5 flex items-center justify-between gap-3">
            <div>
              <h3 className="font-bold text-navy">بيانات التسعير</h3>
              <p className="mt-1 text-xs text-muted-foreground">اختر الخدمة وأدخل تفاصيل التقدير للحصول على خطط سعرية.</p>
            </div>
            <div className="grid h-11 w-11 place-items-center rounded-xl bg-teal-soft text-teal">
              <Wand2 className="h-5 w-5" />
            </div>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2 sm:col-span-2">
              <Label>الخدمة</Label>
              <Select
                dir="rtl"
                value={quoteForm.serviceId}
                disabled={isLoadingLookups}
                onValueChange={(value) => {
                  const service = services.find((item) => item.id === value);
                  setQuoteForm((prev) => ({
                    ...prev,
                    serviceId: value,
                    revision: String(service?.defaultRevisions ?? prev.revision ?? 0),
                  }));
                }}
              >
                <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                  <SelectValue placeholder="اختر الخدمة" />
                </SelectTrigger>
                <SelectContent dir="rtl" className="text-right">
                  {services.map((service) => (
                    <SelectItem key={service.id} value={service.id}>
                      {service.serviceName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="estimatedHours">الساعات المقدرة</Label>
              <Input id="estimatedHours" type="number" min="0.25" step="0.25" required value={quoteForm.estimatedHours} onChange={setQuoteField("estimatedHours")} className="rounded-xl bg-white" />
            </div>

            <div className="space-y-2">
              <Label>التعقيد</Label>
              <Select dir="rtl" value={quoteForm.complexity} onValueChange={(value) => setQuoteForm((prev) => ({ ...prev, complexity: value as ComplexityLevel }))}>
                <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent dir="rtl" className="text-right">
                  <SelectItem value="Simple">بسيط</SelectItem>
                  <SelectItem value="Medium">متوسط</SelectItem>
                  <SelectItem value="Complex">معقد</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="toolCost">تكلفة الأدوات</Label>
              <Input id="toolCost" type="number" min="0" step="0.01" value={quoteForm.toolCost} onChange={setQuoteField("toolCost")} className="rounded-xl bg-white" />
            </div>

            <div className="space-y-2">
              <Label htmlFor="revision">عدد التعديلات</Label>
              <Input id="revision" type="number" min="0" step="1" value={quoteForm.revision} onChange={setQuoteField("revision")} className="rounded-xl bg-white" />
            </div>

            <div className="space-y-2">
              <Label htmlFor="recentProjectCount">عدد المشاريع السابقة</Label>
              <Input id="recentProjectCount" type="number" min="0" max="20" step="1" value={quoteForm.recentProjectCount} onChange={setQuoteField("recentProjectCount")} className="rounded-xl bg-white" />
            </div>

            <label className="flex items-center gap-2 pt-8 text-sm text-muted-foreground" dir="rtl" >
              <Switch dir="rtl" checked={quoteForm.isUrgent} onCheckedChange={(checked) => setQuoteForm((prev) => ({ ...prev, isUrgent: checked }))} />
              تسليم عاجل
            </label>
          </div>

          <Button type="submit" disabled={isCalculating || isLoadingLookups || !quoteForm.serviceId} className="mt-5 rounded-xl bg-gradient-brand shadow-glow hover:opacity-90">
            {isCalculating ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : <Sparkles className="ml-2 h-4 w-4" />}
            احسب السعر
          </Button>
        </form>

        <aside className="space-y-4">
          <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
            <div className="flex items-center gap-3">
              <div className="grid h-11 w-11 place-items-center rounded-xl bg-teal-soft text-teal">
                <TrendingUp className="h-5 w-5" />
              </div>
              <div>
                <div className="text-xs text-muted-foreground">السعر الموصى به</div>
                <div className="text-2xl font-bold text-navy">{formatCurrency(recommendedPlan?.price)}</div>
              </div>
            </div>
            <div className="mt-4 grid grid-cols-2 gap-3 text-sm">
              <div className="rounded-xl bg-muted/40 p-3">
                <div className="text-xs text-muted-foreground">الهامش</div>
                <div className="mt-1 font-bold text-navy">{formatNumber(recommendedPlan?.marginPercent)}%</div>
              </div>
              <div className="rounded-xl bg-muted/40 p-3">
                <div className="text-xs text-muted-foreground">العربون</div>
                <div className="mt-1 font-bold text-navy">{formatCurrency(recommendedPlan?.advanceAmount)}</div>
              </div>
            </div>
          </div>

          <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
            <div className="flex items-center gap-3">
              <div className="grid h-11 w-11 place-items-center rounded-xl bg-teal-soft text-teal">
                <CalendarDays className="h-5 w-5" />
              </div>
              <div>
                <div className="text-xs text-muted-foreground">الساعات بعد التعديل</div>
                <div className="text-2xl font-bold text-navy">{formatNumber(quote?.adjustedHours)} ساعة</div>
              </div>
            </div>
            <div className="mt-4 text-xs leading-relaxed text-muted-foreground">
              السعر الخام: {formatCurrency(quote?.naivePrice)}، أقل سعر مقبول: {formatCurrency(quote?.minAcceptablePrice)}
            </div>
          </div>
        </aside>
      </section>

      {quote && (
        <>
          <section ref={resultsRef} className="grid scroll-mt-32 gap-4 md:grid-cols-3">
            {quote.plans.map((plan) => {
              const normalizedPlan = normalizePlanType(plan.planType);
              const isSelected = createForm.selectedPlan === normalizedPlan;

              return (
                <button
                  key={normalizedPlan}
                  type="button"
                  onClick={() => setCreateForm((prev) => ({ ...prev, selectedPlan: normalizedPlan }))}
                  className={`rounded-2xl border bg-card p-5 text-right shadow-card transition ${
                    isSelected ? "border-teal ring-2 ring-teal/20" : "border-border/70 hover:border-teal/50"
                  }`}
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <div className="font-bold text-navy">{planLabel(plan.planType)}</div>
                      <div className="mt-1 text-xs text-muted-foreground">{plan.isViable ? "قابل للتنفيذ" : "أقل من الهامش الآمن"}</div>
                    </div>
                    <span className={`rounded-full px-2 py-1 text-xs font-bold ${plan.isViable ? "bg-success-soft text-success" : "bg-danger-soft text-danger"}`}>
                      {formatNumber(plan.marginPercent)}%
                    </span>
                  </div>
                  <div className="mt-5 text-2xl font-bold text-teal">{formatCurrency(plan.price)}</div>
                  <div className="mt-2 text-xs text-muted-foreground">عربون مقترح: {formatCurrency(plan.advanceAmount)}</div>
                </button>
              );
            })}
          </section>

          <section className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_420px]">
            <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
              <h3 className="font-bold text-navy">تفاصيل التحليل</h3>
              <div className="mt-4 grid gap-3 text-sm sm:grid-cols-2 lg:grid-cols-3">
                <Metric label="الخدمة" value={quote.serviceName} />
                <Metric label="التعقيد" value={complexityLabel(quoteForm.complexity)} />
                <Metric label="تكلفة التنفيذ" value={formatCurrency(quote.realCost)} />
                <Metric label="سعر الساعة" value={formatCurrency(quote.adjustments.serviceHourlyRate)} />
                <Metric label="معامل التعقيد" value={`${formatNumber(quote.adjustments.complexityMultiplier, 2)}x`} />
                <Metric label="معامل التاريخ" value={`${formatNumber(quote.adjustments.historicalHoursFactor, 2)}x`} />
                <Metric label="تعديلات إضافية" value={String(quote.adjustments.extraRevisions)} />
                <Metric label="مشاريع مكتملة" value={String(quote.history.completedProjectCount)} />
                <Metric label="متوسط الهامش السابق" value={`${formatNumber(quote.history.averageMarginPercent)}%`} />
              </div>

              <div className="mt-5 space-y-2">
                {quote.insights.map((insight, index) => (
                  <div key={`${insight.message}-${index}`} className={`rounded-xl border p-3 text-sm ${insightClass(insight.severity)}`}>
                    {insight.message}
                  </div>
                ))}
              </div>
            </div>

            <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
              <div className="mb-4 flex items-center justify-between gap-3">
                <h3 className="font-bold text-navy">تحويل العرض</h3>
                <FileText className="h-5 w-5 text-teal" />
              </div>

              <div className="space-y-3">
                <div className="space-y-2">
                  <Label>العميل</Label>
                  <Select dir="rtl" value={createForm.customerId} onValueChange={(value) => setCreateForm((prev) => ({ ...prev, customerId: value }))}>
                    <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                      <SelectValue placeholder="اختر العميل" />
                    </SelectTrigger>
                    <SelectContent dir="rtl" className="text-right">
                      {customers.map((customer) => (
                        <SelectItem key={customer.id} value={customer.id}>
                          {customer.customerName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="projectName">اسم المشروع</Label>
                  <Input id="projectName" value={createForm.projectName} onChange={(event) => setCreateForm((prev) => ({ ...prev, projectName: event.target.value }))} className="rounded-xl bg-white" />
                </div>

                <div className="grid gap-3 sm:grid-cols-2">
                  <div className="space-y-2">
                    <Label htmlFor="startDate">تاريخ البداية</Label>
                    <Input id="startDate" type="date" value={createForm.startDate} onChange={(event) => setCreateForm((prev) => ({ ...prev, startDate: event.target.value }))} className="rounded-xl bg-white" />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="endDate">تاريخ النهاية</Label>
                    <Input id="endDate" type="date" value={createForm.endDate} onChange={(event) => setCreateForm((prev) => ({ ...prev, endDate: event.target.value }))} className="rounded-xl bg-white" />
                  </div>
                </div>

                <div className="rounded-xl border border-border/70 bg-muted/40 p-3 text-sm">
                  <div className="text-xs text-muted-foreground">الخطة المحددة</div>
                  <div className="mt-1 font-bold text-navy">
                    {planLabel(createForm.selectedPlan)} - {formatCurrency(selectedPlan?.price)}
                  </div>
                </div>

                <Button type="button" disabled={isCreatingProject || !createForm.customerId || !createForm.projectName} onClick={() => void handleCreateProject()} className="w-full rounded-xl bg-gradient-brand shadow-glow hover:opacity-90">
                  {isCreatingProject ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : null}
                  إنشاء مشروع
                </Button>

                <div className="grid gap-3 sm:grid-cols-2">
                  <div className="space-y-2">
                    <Label htmlFor="invoiceNumber">رقم الفاتورة</Label>
                    <Input id="invoiceNumber" value={createForm.invoiceNumber} onChange={(event) => setCreateForm((prev) => ({ ...prev, invoiceNumber: event.target.value }))} className="rounded-xl bg-white" />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="currency">العملة</Label>
                    <Input id="currency" value={createForm.currency} onChange={(event) => setCreateForm((prev) => ({ ...prev, currency: event.target.value.toUpperCase() }))} className="rounded-xl bg-white" />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="issueDate">تاريخ الإصدار</Label>
                    <Input id="issueDate" type="date" value={createForm.issueDate} onChange={(event) => setCreateForm((prev) => ({ ...prev, issueDate: event.target.value }))} className="rounded-xl bg-white" />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="dueDate">تاريخ الاستحقاق</Label>
                    <Input id="dueDate" type="date" value={createForm.dueDate} onChange={(event) => setCreateForm((prev) => ({ ...prev, dueDate: event.target.value }))} className="rounded-xl bg-white" />
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="notes">ملاحظات</Label>
                  <Input id="notes" value={createForm.notes} onChange={(event) => setCreateForm((prev) => ({ ...prev, notes: event.target.value }))} className="rounded-xl bg-white" />
                </div>

                <Button type="button" variant="outline" disabled={isCreatingInvoice || !createForm.customerId || !createForm.projectName || !createForm.invoiceNumber} onClick={() => void handleCreateInvoice()} className="w-full rounded-xl">
                  {isCreatingInvoice ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : null}
                  إنشاء فاتورة
                </Button>
              </div>
            </div>
          </section>

          <section className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
            <h3 className="font-bold text-navy">آخر المشاريع المستخدمة في التحليل</h3>
            {quote.recentProjects.length ? (
              <div className="mt-4 overflow-hidden rounded-xl border border-border/70">
                <table className="w-full text-right text-sm">
                  <thead className="bg-muted/50 text-xs text-muted-foreground">
                    <tr>
                      <th className="px-4 py-3 font-semibold">المشروع</th>
                      <th className="px-4 py-3 font-semibold">المقدر</th>
                      <th className="px-4 py-3 font-semibold">الفعلي</th>
                      <th className="px-4 py-3 font-semibold">السعر</th>
                      <th className="px-4 py-3 font-semibold">الهامش</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border/70">
                    {quote.recentProjects.map((project) => (
                      <tr key={project.projectId}>
                        <td className="px-4 py-3 font-semibold text-navy">{project.projectName}</td>
                        <td className="px-4 py-3 text-muted-foreground">{formatNumber(project.estimatedHours)} ساعة</td>
                        <td className="px-4 py-3 text-muted-foreground">{formatNumber(project.actualHours)} ساعة</td>
                        <td className="px-4 py-3 text-muted-foreground">{formatCurrency(project.suggestedPrice)}</td>
                        <td className="px-4 py-3 text-muted-foreground">{formatNumber(project.actualMarginPercent)}%</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <div className="mt-4 rounded-xl bg-muted/40 p-8 text-center text-sm text-muted-foreground">
                لا توجد مشاريع مكتملة لهذه الخدمة بعد.
              </div>
            )}
          </section>
        </>
      )}
    </>
  );
}

function Metric({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
      <div className="text-xs text-muted-foreground">{label}</div>
      <div className="mt-1 font-semibold text-navy">{value}</div>
    </div>
  );
}
