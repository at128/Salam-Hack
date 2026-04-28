import { useEffect, useMemo, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { ArrowRight, Bot, Loader2, Sparkles } from "lucide-react";
import { PageHeader } from "@/components/dashboard/DashboardLayout";
import {
  computeRisk,
  GEMMA_MODEL,
  requestGemmaAnalysis,
  RISK_CONFIG,
  type FormState,
  type GemmaRiskAnalysis,
} from "@/pages/dashboard/ClientRiskAnalyzer";

type AnalysisStatus = "loading" | "success" | "error";

function NumberCard({ label, value, tone = "text-navy" }: { label: string; value: string; tone?: string }) {
  return (
    <div className="rounded-2xl border border-border/70 bg-white p-5 shadow-card">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className={`mt-2 text-3xl font-bold ${tone}`}>{value}</p>
    </div>
  );
}

function SectionList({ title, items }: { title: string; items: string[] }) {
  if (!items.length) return null;

  return (
    <section className="rounded-2xl border border-border/70 bg-card p-6 shadow-card">
      <h3 className="text-base font-bold text-navy">{title}</h3>
      <ul className="mt-4 grid gap-3">
        {items.map((item, index) => (
          <li key={item} className="flex gap-3 rounded-xl bg-muted/40 p-3 text-sm leading-relaxed text-navy">
            <span className="grid h-6 w-6 shrink-0 place-items-center rounded-full bg-teal text-xs font-bold text-white">
              {index + 1}
            </span>
            {item}
          </li>
        ))}
      </ul>
    </section>
  );
}

export default function ClientRiskResultPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const form = (location.state as { form?: FormState } | null)?.form;
  const localResult = useMemo(() => (form ? computeRisk(form) : null), [form]);
  const [status, setStatus] = useState<AnalysisStatus>("loading");
  const [analysis, setAnalysis] = useState<GemmaRiskAnalysis | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!form || !localResult) {
      navigate("/dashboard/client-risk", { replace: true });
      return;
    }

    let active = true;
    setStatus("loading");
    setError("");
    setAnalysis(null);

    requestGemmaAnalysis(form, localResult)
      .then((result) => {
        if (!active) return;
        setAnalysis(result);
        setStatus("success");
      })
      .catch((err) => {
        if (!active) return;
        setError(err instanceof Error ? err.message : "تعذر الاتصال بخدمة Gemma 4.");
        setStatus("error");
      });

    return () => {
      active = false;
    };
  }, [form, localResult, navigate]);

  if (!form || !localResult) return null;

  const fallbackConfig = RISK_CONFIG[localResult.level];
  const activeLevel = status === "loading" ? "medium" : analysis?.riskLevel ?? localResult.level;
  const activeConfig = RISK_CONFIG[activeLevel];
  const fallbackScore = Math.round((localResult.score / 7) * 100);
  const isFallback = status === "error";
  const resultTone = activeLevel === "low" ? "text-success" : activeLevel === "high" ? "text-danger" : "text-warning";
  const resultAccent = activeLevel === "low" ? "bg-success" : activeLevel === "high" ? "bg-danger" : "bg-warning";

  return (
    <>
      <div className="mb-5 flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
        <PageHeader
          title="نتيجة تحليل العميل"
          desc="تقرير مفصل يساعدك على اتخاذ قرار التعامل وشروط الدفع قبل بدء المشروع."
        />
        <Link
          to="/dashboard/client-risk"
          className="inline-flex items-center justify-center gap-2 rounded-xl border border-border/70 bg-card px-4 py-2 text-sm font-semibold text-navy transition-colors hover:bg-muted/50"
        >
          <ArrowRight className="h-4 w-4" />
          تحليل عميل آخر
        </Link>
      </div>

      <section className={`overflow-hidden rounded-3xl border ${activeConfig.border} ${activeConfig.bg} shadow-card`}>
        <div className="p-4 lg:p-6">
          <div className="rounded-2xl border border-border/70 bg-white p-5 shadow-card lg:p-7">
            <div className="max-w-4xl">
              <div className="mb-5 inline-flex items-center gap-2 rounded-full border border-border/70 bg-muted/35 px-3 py-1 text-xs font-semibold text-navy">
                {status === "loading" ? <Loader2 className="h-3.5 w-3.5 animate-spin" /> : <Bot className="h-3.5 w-3.5" />}
                {status === "loading" ? "جاري إعداد التقرير عبر Gemma 4" : isFallback ? "تحليل منطقي احتياطي" : "تحليل Gemma 4 الاحترافي"}
              </div>
              <div className="flex items-start gap-3">
                {status !== "loading" && <span className={`mt-2 h-8 w-1.5 rounded-full ${resultAccent}`} />}
                <h2 className={`text-3xl font-bold ${status === "loading" ? "text-navy" : resultTone}`}>
                  {status === "loading" ? "جاري تحليل مؤشرات العميل" : activeConfig.label}
                </h2>
              </div>
              <p className="mt-3 max-w-2xl text-sm leading-7 text-muted-foreground">
                {status === "loading"
                  ? "يتم تحليل إجابات العميل وإعداد التقرير النهائي."
                  : isFallback
                    ? "تعذر عرض تحليل Gemma 4، لذلك نعرض التحليل المنطقي المحلي كخطة احتياطية."
                    : analysis?.executiveSummary}
              </p>
              {isFallback && (
                <p className="mt-3 rounded-xl border border-warning/40 bg-white/60 p-3 text-xs leading-relaxed text-muted-foreground">
                  {error}
                </p>
              )}

              {status !== "loading" && (
                <div className="mt-6 grid gap-3 sm:grid-cols-2">
                  <NumberCard
                    label="درجة المخاطر"
                    value={`${isFallback ? fallbackScore : analysis?.riskScore}%`}
                    tone={resultTone}
                  />
                  {!isFallback && analysis && <NumberCard label="مستوى الثقة" value={`${analysis.confidence}%`} />}
                </div>
              )}
            </div>
          </div>
        </div>
      </section>

      {status === "loading" ? (
        <div className="mt-6 rounded-2xl border border-border/70 bg-card p-8 text-center shadow-card">
          <Loader2 className="mx-auto h-8 w-8 animate-spin text-teal" />
          <p className="mt-3 text-sm font-semibold text-navy">يتم تجهيز صفحة النتيجة...</p>
        </div>
      ) : isFallback ? (
        <div className="mt-6 grid gap-6 lg:grid-cols-2">
          <SectionList title="عوامل الخطورة المكتشفة" items={localResult.factors} />
          <SectionList title="توصيات مالية" items={fallbackConfig.recommendations} />
        </div>
      ) : analysis ? (
        <div className="mt-6 grid gap-6 lg:grid-cols-2">
          <SectionList title="أبرز المخاطر" items={analysis.keyRisks} />
          <SectionList title="توصيات عملية" items={analysis.recommendations} />
          <SectionList title="بنود تعاقدية مقترحة" items={analysis.contractTerms} />
          <SectionList title="الخطوات التالية" items={analysis.nextSteps} />

          <section className="rounded-2xl border border-border/70 bg-card p-6 shadow-card lg:col-span-2">
            <div className="flex items-center gap-3">
              <div className="grid h-11 w-11 place-items-center rounded-xl bg-teal-soft text-teal">
                <Sparkles className="h-5 w-5" />
              </div>
              <div>
                <h3 className="font-bold text-navy">مصدر التقرير</h3>
                <p className="text-xs text-muted-foreground">{GEMMA_MODEL}</p>
              </div>
            </div>
          </section>
        </div>
      ) : null}
    </>
  );
}
