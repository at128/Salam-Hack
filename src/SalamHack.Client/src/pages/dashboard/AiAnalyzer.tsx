import { useEffect, useMemo, useState } from "react";
import { AlertTriangle, Bot, CheckCircle2, Lightbulb, Loader2, RefreshCw, Sparkles, TrendingUp } from "lucide-react";
import { PageHeader } from "@/components/dashboard/DashboardLayout";
import { Button } from "@/components/ui/button";
import { getApiErrorMessage, getValidAccessToken, unwrapApiResponse } from "@/lib/auth";

type ProfitAiResponse = {
  content: string;
  generatedAt: string;
};

type AiRecommendation = {
  title?: string;
  description?: string;
  priority?: "high" | "medium" | "low" | string;
};

type AiAlert = {
  title?: string;
  description?: string;
  actionLabel?: string;
  severity?: "critical" | "warning" | "success" | "info" | string;
};

type AiOpportunity = {
  title?: string;
  description?: string;
  impact?: "high" | "medium" | "low" | string;
};

type ProfitAiAnalysis = {
  executiveSummary?: string;
  recommendations?: AiRecommendation[];
  alerts?: AiAlert[];
  opportunities?: AiOpportunity[];
};

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const AI_PROFIT_API_URL = `${API_BASE_URL}/api/v1/ai/profit`;

async function fetchProfitAiAnalysis(): Promise<ProfitAiResponse> {
  const token = await getValidAccessToken();
  if (!token) throw new Error("Missing access token.");

  const response = await fetch(AI_PROFIT_API_URL, {
    method: "POST",
    headers: {
      Accept: "application/json",
      Authorization: `Bearer ${token}`,
    },
  });

  const payload = await response.json().catch(() => null);

  if (!response.ok) {
    throw new Error(getApiErrorMessage(payload, "تعذر تشغيل محلل الأرباح الذكي."));
  }

  return unwrapApiResponse<ProfitAiResponse>(payload);
}

function extractJsonObject(text: string) {
  const fenced = text.match(/```(?:json)?\s*([\s\S]*?)```/i)?.[1];
  const candidate = fenced ?? text;
  const start = candidate.indexOf("{");
  const end = candidate.lastIndexOf("}");

  if (start < 0 || end <= start) return candidate.trim();

  return candidate.slice(start, end + 1);
}

function parseAnalysis(content: string): ProfitAiAnalysis {
  try {
    const parsed = JSON.parse(extractJsonObject(content)) as ProfitAiAnalysis;
    return {
      executiveSummary: parsed.executiveSummary,
      recommendations: Array.isArray(parsed.recommendations) ? parsed.recommendations : [],
      alerts: Array.isArray(parsed.alerts) ? parsed.alerts : [],
      opportunities: Array.isArray(parsed.opportunities) ? parsed.opportunities : [],
    };
  } catch {
    return {
      executiveSummary: content,
      recommendations: [],
      alerts: [],
      opportunities: [],
    };
  }
}

function formatDateTime(value?: string) {
  if (!value) return "";

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";

  return new Intl.DateTimeFormat("ar", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function priorityLabel(value?: string) {
  switch (value?.toLowerCase()) {
    case "high":
      return "أولوية عالية";
    case "medium":
      return "أولوية متوسطة";
    case "low":
      return "أولوية منخفضة";
    default:
      return "توصية";
  }
}

function impactLabel(value?: string) {
  switch (value?.toLowerCase()) {
    case "high":
      return "أثر مرتفع";
    case "medium":
      return "أثر متوسط";
    case "low":
      return "أثر منخفض";
    default:
      return "فرصة";
  }
}

function severityStyles(value?: string) {
  switch (value?.toLowerCase()) {
    case "critical":
      return "border-danger/30 bg-danger-soft text-danger";
    case "warning":
      return "border-warning/30 bg-warning-soft text-warning";
    case "success":
      return "border-success/30 bg-success-soft text-success";
    default:
      return "border-teal/25 bg-teal-soft text-teal";
  }
}

function priorityStyles(value?: string) {
  switch (value?.toLowerCase()) {
    case "high":
      return "bg-danger-soft text-danger";
    case "medium":
      return "bg-warning-soft text-warning";
    case "low":
      return "bg-success-soft text-success";
    default:
      return "bg-muted text-muted-foreground";
  }
}

export default function AiAnalyzerPage() {
  const [analysisResponse, setAnalysisResponse] = useState<ProfitAiResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");

  const loadAnalysis = async () => {
    setIsLoading(true);
    setError("");

    try {
      const result = await fetchProfitAiAnalysis();
      setAnalysisResponse(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : "تعذر تشغيل محلل الأرباح الذكي.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void loadAnalysis();
  }, []);

  const analysis = useMemo(
    () => (analysisResponse?.content ? parseAnalysis(analysisResponse.content) : null),
    [analysisResponse],
  );

  const recommendations = analysis?.recommendations ?? [];
  const alerts = analysis?.alerts ?? [];
  const opportunities = analysis?.opportunities ?? [];

  return (
    <>
      <PageHeader
        title="محلل الأرباح الذكي"
        desc="رؤى مبنية على بياناتك الفعلية لتنمية دخلك."
      />

      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="rounded-xl border border-border/70 bg-card px-4 py-3 text-sm text-muted-foreground shadow-card">
          {analysisResponse?.generatedAt ? `آخر تحديث: ${formatDateTime(analysisResponse.generatedAt)}` : "محدّثة بناءً على آخر بياناتك"}
        </div>
        <Button
          type="button"
          onClick={() => void loadAnalysis()}
          disabled={isLoading}
          className="rounded-xl bg-teal text-white hover:bg-teal/90"
        >
          {isLoading ? <Loader2 className="h-4 w-4 animate-spin" /> : <RefreshCw className="h-4 w-4" />}
          تحديث التحليل
        </Button>
      </div>

      {error && (
        <div className="rounded-xl border border-danger/30 bg-danger-soft p-4 text-sm text-danger">
          {error}
        </div>
      )}

      {isLoading ? (
        <div className="rounded-2xl border border-border/70 bg-card p-8 text-center text-sm text-muted-foreground shadow-card">
          <Loader2 className="mx-auto mb-3 h-6 w-6 animate-spin text-teal" />
          جاري تحليل الأرباح والدفعات والمشاريع...
        </div>
      ) : (
        <div className="grid gap-6 lg:grid-cols-3">
          <section className="lg:col-span-2 space-y-6">
            <div className="rounded-2xl border border-border/70 bg-card p-6 shadow-card">
              <div className="mb-4 flex items-center gap-3">
                <div className="grid h-11 w-11 place-items-center rounded-xl bg-gradient-brand text-white shadow-glow">
                  <Bot className="h-5 w-5" />
                </div>
                <div>
                  <h3 className="font-bold text-navy">توصيات مالية</h3>
                  <p className="text-xs text-muted-foreground">مولّدة من بيانات الإيرادات والتحصيل وهوامش المشاريع.</p>
                </div>
              </div>

              {analysis?.executiveSummary && (
                <p className="mb-4 rounded-xl bg-muted/40 p-4 text-sm leading-relaxed text-navy">
                  {analysis.executiveSummary}
                </p>
              )}

              <div className="space-y-3">
                {recommendations.length > 0 ? (
                  recommendations.map((item, index) => (
                    <article key={`${item.title}-${index}`} className="flex gap-3 rounded-xl bg-muted/40 p-4 text-sm leading-relaxed text-navy">
                      <span className="grid h-7 w-7 shrink-0 place-items-center rounded-full bg-teal text-xs font-bold text-white">
                        {index + 1}
                      </span>
                      <div className="min-w-0 flex-1">
                        <div className="mb-1 flex flex-wrap items-center gap-2">
                          <h4 className="font-bold">{item.title ?? "توصية مالية"}</h4>
                          <span className={`rounded-full px-2 py-1 text-xs font-bold ${priorityStyles(item.priority)}`}>
                            {priorityLabel(item.priority)}
                          </span>
                        </div>
                        <p className="text-muted-foreground">{item.description}</p>
                      </div>
                    </article>
                  ))
                ) : (
                  <div className="rounded-xl bg-muted/50 p-6 text-sm text-muted-foreground">
                    لا توجد توصيات متاحة حاليا.
                  </div>
                )}
              </div>
            </div>

            <div className="rounded-2xl border border-border/70 bg-card p-6 shadow-card">
              <div className="mb-4 flex items-center gap-2">
                <TrendingUp className="h-5 w-5 text-teal" />
                <h3 className="font-bold text-navy">فرص نمو</h3>
              </div>
              <div className="grid gap-3 sm:grid-cols-2">
                {opportunities.length > 0 ? (
                  opportunities.map((item, index) => (
                    <article key={`${item.title}-${index}`} className="rounded-xl border border-border/70 p-4">
                      <div className="mb-2 flex items-center justify-between gap-2">
                        <h4 className="font-bold text-navy">{item.title ?? "فرصة نمو"}</h4>
                        <span className={`shrink-0 rounded-full px-2 py-1 text-xs font-bold ${priorityStyles(item.impact)}`}>
                          {impactLabel(item.impact)}
                        </span>
                      </div>
                      <p className="text-sm leading-relaxed text-muted-foreground">{item.description}</p>
                    </article>
                  ))
                ) : (
                  <div className="rounded-xl bg-muted/50 p-6 text-sm text-muted-foreground sm:col-span-2">
                    لا توجد فرص نمو متاحة حاليا.
                  </div>
                )}
              </div>
            </div>
          </section>

          <aside className="rounded-2xl border border-border/70 bg-card p-6 shadow-card">
            <div className="mb-4 flex items-center gap-2">
              <Sparkles className="h-5 w-5 text-teal" />
              <h3 className="font-bold text-navy">تنبيهات ذكية</h3>
            </div>
            <div className="space-y-3">
              {alerts.length > 0 ? (
                alerts.map((alert, index) => {
                  const isSuccess = alert.severity?.toLowerCase() === "success";
                  const Icon = isSuccess ? CheckCircle2 : AlertTriangle;

                  return (
                    <article
                      key={`${alert.title}-${index}`}
                      className={`rounded-xl border-r-4 p-4 ${severityStyles(alert.severity)}`}
                    >
                      <div className="mb-2 flex items-start gap-2">
                        <Icon className="mt-0.5 h-5 w-5 shrink-0" />
                        <div className="min-w-0">
                          <h4 className="font-bold text-navy">{alert.title ?? "تنبيه"}</h4>
                          <p className="mt-1 text-sm leading-relaxed text-navy/75">{alert.description}</p>
                        </div>
                      </div>
                      {alert.actionLabel && (
                        <Button type="button" size="sm" variant="outline" className="mt-2 h-8 rounded-full bg-white text-xs">
                          <Lightbulb className="h-4 w-4" />
                          {alert.actionLabel}
                        </Button>
                      )}
                    </article>
                  );
                })
              ) : (
                <div className="rounded-xl bg-muted/50 p-6 text-sm text-muted-foreground">
                  لا توجد تنبيهات متاحة حاليا.
                </div>
              )}
            </div>
          </aside>
        </div>
      )}
    </>
  );
}
