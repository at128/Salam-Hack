import { useState } from "react";
import {
  Bot,
  Loader2,
  ShieldCheck,
  ShieldAlert,
  ShieldX,
  Sparkles,
  ArrowLeft,
  RotateCcw,
} from "lucide-react";

export type RiskLevel = "low" | "medium" | "high";
type AnalysisStatus = "idle" | "loading" | "success" | "error";
type Stage = "intro" | "questions" | "loading" | "result";

export interface FormState {
  paymentRecord: string;
  priorProjects: string;
  scopeClarity: string;
  revisions: string;
  communication: string;
  budgetCommitment: string;
}

export interface GemmaRiskAnalysis {
  executiveSummary: string;
  riskLevel: RiskLevel;
  riskScore: number;
  confidence: number;
  keyRisks: string[];
  recommendations: string[];
  contractTerms: string[];
  nextSteps: string[];
}

const INITIAL: FormState = {
  paymentRecord: "",
  priorProjects: "",
  scopeClarity: "",
  revisions: "",
  communication: "",
  budgetCommitment: "",
};

export function computeRisk(form: FormState): { level: RiskLevel; score: number; factors: string[] } | null {
  const { paymentRecord, priorProjects, revisions, communication, budgetCommitment } = form;
  if (!paymentRecord || !priorProjects || !revisions || !communication || !budgetCommitment) return null;

  let score = 0;
  const factors: string[] = [];

  const payScores: Record<string, number> = { ontime: 0, sometimes: 1, frequent: 2 };
  const priorScores: Record<string, number> = { many: 0, one: 0.5, first: 1 };
  const revScores: Record<string, number> = { few: 0, medium: 0.5, many: 1, unlimited: 1 };
  const commScores: Record<string, number> = { clear: 0, sometimes: 0.5, difficult: 1, unavailable: 1 };
  const budgetScores: Record<string, number> = { committed: 0, negotiates: 1, refuses: 2, disputed: 2 };

  score += payScores[paymentRecord];
  score += priorScores[priorProjects];
  score += revScores[revisions];
  score += commScores[communication];
  score += budgetScores[budgetCommitment];

  if (paymentRecord === "sometimes") factors.push("سجل تأخر أحياناً في الدفع");
  if (paymentRecord === "frequent") factors.push("تأخر متكرر في الدفع");
  if (priorProjects === "first") factors.push("عميل جديد بدون سجل سابق");
  if (revisions === "many") factors.push("يطلب تعديلات كثيرة وغير محدودة");
  if (communication === "difficult") factors.push("أسلوب تواصل صعب أو غير واضح");
  if (budgetCommitment === "negotiates") factors.push("يفاوض على الأسعار بعد الاتفاق");
  if (budgetCommitment === "refuses") factors.push("تاريخ في المماطلة أو رفض الدفع");

  const level: RiskLevel = score <= 2 ? "low" : score <= 4 ? "medium" : "high";
  return { level, score, factors };
}

export const GEMMA_MODEL = "google/gemma-4-31b-it";
const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const CLIENT_RISK_API_URL = `${API_BASE_URL}/api/v1/clientrisk/analyze`;
const ANSWER_LABELS: Record<keyof FormState, Record<string, string>> = {
  paymentRecord: {
    ontime: "Always pays on time",
    sometimes: "Sometimes pays late",
    frequent: "Frequently delays or avoids payment",
  },
  priorProjects: {
    many: "Completed two or more paid projects with this client",
    one: "Completed one paid project with this client",
    first: "No previous paid work with this client",
  },
  scopeClarity: {
    clearWritten: "Provided a clear written scope before starting",
    partial: "Provided general requirements but some details are missing",
    changing: "Requirements changed more than once before starting",
    unclear: "No clear scope or expected deliverables yet",
  },
  revisions: {
    few: "Usually asks for one or two specific revision rounds",
    medium: "Usually asks for three or four revision rounds",
    many: "Often asks for extra revisions after approval",
    unlimited: "Requests open-ended or unlimited revisions",
  },
  communication: {
    clear: "Replies within one business day with clear answers",
    sometimes: "Replies within two or three days and answers most points",
    difficult: "Often ignores some questions or gives incomplete answers",
    unavailable: "Frequently disappears for several days without notice",
  },
  budgetCommitment: {
    committed: "Confirmed the price and payment terms in writing",
    negotiates: "Asked to reduce the price after seeing the scope",
    refuses: "Avoids confirming payment terms before work starts",
    disputed: "Previously disputed an agreed payment or invoice",
  },
};

function buildGemmaPrompt(form: FormState, localResult: NonNullable<ReturnType<typeof computeRisk>>) {
  return `You are a senior client risk advisor for freelancers and small service businesses.
Analyze this client intake professionally. Return only valid JSON with no markdown.

Context:
- Local heuristic risk level: ${localResult.level}
- Local heuristic score: ${localResult.score} out of 7
- Payment record: ${ANSWER_LABELS.paymentRecord[form.paymentRecord]}
- Prior projects: ${ANSWER_LABELS.priorProjects[form.priorProjects]}
- Scope clarity: ${ANSWER_LABELS.scopeClarity[form.scopeClarity]}
- Revision behavior: ${ANSWER_LABELS.revisions[form.revisions]}
- Communication: ${ANSWER_LABELS.communication[form.communication]}
- Budget commitment: ${ANSWER_LABELS.budgetCommitment[form.budgetCommitment]}

JSON schema:
{
  "executiveSummary": "Arabic business summary in 1-2 sentences",
  "riskLevel": "low | medium | high",
  "riskScore": number from 0 to 100,
  "confidence": number from 0 to 100,
  "keyRisks": ["Arabic risk factor", "..."],
  "recommendations": ["Arabic practical recommendation", "..."],
  "contractTerms": ["Arabic contract/payment term", "..."],
  "nextSteps": ["Arabic next action", "..."]
}

Rules:
- Write all user-facing values in Arabic.
- Keep the tone professional, concise, and suitable for a financial dashboard.
- Do not invent private facts beyond the provided answers.
- Provide 3-5 items for each array.
- Match riskLevel to the evidence unless there is a clear reason to adjust it.`;
}

function extractJson(text: string) {
  const cleaned = text.trim().replace(/^```(?:json)?\s*/i, "").replace(/\s*```$/i, "");
  const start = cleaned.indexOf("{");
  const end = cleaned.lastIndexOf("}");
  if (start === -1 || end === -1 || end <= start) throw new Error("Gemma did not return a JSON object.");
  return cleaned.slice(start, end + 1);
}

function normalizeRiskLevel(value: unknown, fallback: RiskLevel): RiskLevel {
  return value === "low" || value === "medium" || value === "high" ? value : fallback;
}

function toStringArray(value: unknown) {
  return Array.isArray(value) ? value.filter((item): item is string => typeof item === "string") : [];
}

function clampPercent(value: unknown, fallback: number) {
  const n = typeof value === "number" ? value : Number(value);
  if (!Number.isFinite(n)) return fallback;
  return Math.max(0, Math.min(100, Math.round(n)));
}

export async function requestGemmaAnalysis(
  form: FormState,
  localResult: NonNullable<ReturnType<typeof computeRisk>>,
): Promise<GemmaRiskAnalysis> {
  try {
    const response = await fetch(CLIENT_RISK_API_URL, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ prompt: buildGemmaPrompt(form, localResult) }),
    });

    if (!response.ok) throw new Error("Gemma API request failed.");

    const payload = await response.json();
    const text = payload?.content;
    if (!text || typeof text !== "string") throw new Error("Gemma did not return analysis text.");

    const parsed = JSON.parse(extractJson(text));
    return {
      executiveSummary: String(parsed.executiveSummary ?? ""),
      riskLevel: normalizeRiskLevel(parsed.riskLevel, localResult.level),
      riskScore: clampPercent(parsed.riskScore, Math.round((localResult.score / 7) * 100)),
      confidence: clampPercent(parsed.confidence, 70),
      keyRisks: toStringArray(parsed.keyRisks),
      recommendations: toStringArray(parsed.recommendations),
      contractTerms: toStringArray(parsed.contractTerms),
      nextSteps: toStringArray(parsed.nextSteps),
    };
  } catch {
    throw new Error("Gemma API request failed.");
  }
}

export const RISK_CONFIG = {
  low: {
    label: "مستوى خطورة منخفض",
    sublabel: "التعامل مع هذا العميل آمن ومطمئن",
    icon: ShieldCheck,
    textColor: "text-emerald-700",
    bg: "bg-success-soft",
    border: "border-success/40",
    barColor: "bg-success",
    recommendations: [
      "تابع التعامل مع هذا العميل بثقة.",
      "وثّق العقود دائماً للحفاظ على العلاقة المهنية.",
      "يمكنك تقديم شروط دفع مرنة نسبياً.",
    ],
  },
  medium: {
    label: "مستوى خطورة متوسط",
    sublabel: "خذ احتياطاتك قبل بدء التعامل",
    icon: ShieldAlert,
    textColor: "text-amber-700",
    bg: "bg-warning-soft",
    border: "border-warning/40",
    barColor: "bg-warning",
    recommendations: [
      "اشترط دفعة مقدمة لا تقل عن ٥٠٪ قبل البدء.",
      "حدّد عدد التعديلات المسموح بها في العقد.",
      "تابع الدفعات بجدول واضح ومتفق عليه.",
    ],
  },
  high: {
    label: "مستوى خطورة مرتفع",
    sublabel: "كن حذراً جداً في التعامل مع هذا العميل",
    icon: ShieldX,
    textColor: "text-red-700",
    bg: "bg-danger-soft",
    border: "border-danger/40",
    barColor: "bg-danger",
    recommendations: [
      "اشترط دفعاً كاملاً مسبقاً قبل البدء بأي عمل.",
      "لا تبدأ بدون عقد رسمي موقّع.",
      "ضع في اعتبارك رفض العميل إن لم يوافق على شروطك.",
    ],
  },
};

const QUESTIONS = [
  {
    id: "paymentRecord" as keyof FormState,
    label: "سجل الدفع",
    hint: "كيف كان تعامل هذا العميل مع الدفعات أو الفواتير في التجارب السابقة؟",
    options: [
      { value: "ontime", label: "دفع في الموعد المتفق عليه في آخر تعامل" },
      { value: "sometimes", label: "تأخر مرة واحدة أو طلب تمديد موعد الدفع" },
      { value: "frequent", label: "تأخر أكثر من مرة أو احتاج إلى متابعة متكررة" },
    ],
  },
  {
    id: "priorProjects" as keyof FormState,
    label: "سابقة التعامل",
    hint: "كم مرة أنجزت عملاً مدفوعاً مع هذا العميل؟",
    options: [
      { value: "many", label: "مشروعان أو أكثر وتم إغلاقها بدون خلاف" },
      { value: "one", label: "مشروع واحد فقط وتم إغلاقه" },
      { value: "first", label: "لا يوجد تعامل سابق معه" },
    ],
  },
  {
    id: "scopeClarity" as keyof FormState,
    label: "وضوح نطاق العمل",
    hint: "هل المتطلبات والمخرجات المطلوبة واضحة قبل بدء العمل؟",
    options: [
      { value: "clearWritten", label: "أرسل نطاقاً مكتوباً ومخرجات واضحة" },
      { value: "partial", label: "شرح الفكرة العامة لكن توجد تفاصيل ناقصة" },
      { value: "changing", label: "غيّر المتطلبات أكثر من مرة قبل البدء" },
      { value: "unclear", label: "لم يحدد المخرجات أو حدود العمل بوضوح" },
    ],
  },
  {
    id: "revisions" as keyof FormState,
    label: "طلبات التعديل",
    hint: "كيف يتعامل هذا العميل عادة مع التعديلات بعد تسليم العمل؟",
    options: [
      { value: "few", label: "يطلب جولة أو جولتين بتعليقات محددة" },
      { value: "medium", label: "يطلب ثلاث إلى أربع جولات تعديل" },
      { value: "many", label: "يضيف طلبات جديدة بعد الموافقة على التسليم" },
      { value: "unlimited", label: "يتوقع تعديلات مفتوحة بدون حد واضح" },
    ],
  },
  {
    id: "communication" as keyof FormState,
    label: "سرعة ووضوح التواصل",
    hint: "كيف يرد العميل على الأسئلة والرسائل أثناء النقاش؟",
    options: [
      { value: "clear", label: "يرد خلال يوم عمل وبإجابات واضحة" },
      { value: "sometimes", label: "يرد خلال يومين أو ثلاثة ويجيب على أغلب النقاط" },
      { value: "difficult", label: "يتجاهل بعض الأسئلة أو يرسل إجابات ناقصة" },
      { value: "unavailable", label: "ينقطع عن الرد عدة أيام بدون تنبيه" },
    ],
  },
  {
    id: "budgetCommitment" as keyof FormState,
    label: "تأكيد الميزانية وشروط الدفع",
    hint: "هل أكد العميل السعر وطريقة الدفع قبل بدء العمل؟",
    options: [
      { value: "committed", label: "وافق كتابياً على السعر وشروط الدفع" },
      { value: "negotiates", label: "طلب تخفيض السعر بعد معرفة تفاصيل العمل" },
      { value: "refuses", label: "يتجنب تأكيد شروط الدفع قبل البدء" },
      { value: "disputed", label: "سبق أن اعترض على مبلغ متفق عليه أو فاتورة" },
    ],
  },
];

function RadioGroup({
  name,
  options,
  value,
  onChange,
}: {
  name: string;
  options: { value: string; label: string }[];
  value: string;
  onChange: (v: string) => void;
}) {
  return (
    <div className="flex flex-col gap-2">
      {options.map((opt) => {
        const active = value === opt.value;
        return (
          <button
            key={opt.value}
            type="button"
            onClick={() => onChange(opt.value)}
            className={`grid grid-cols-[1rem_1fr_1rem] items-center gap-3 px-4 py-2.5 rounded-xl border text-sm text-center transition-colors ${
              active
                ? "border-teal bg-teal-soft text-teal font-medium"
                : "border-border/60 bg-card text-navy hover:border-teal/40 hover:bg-teal-soft/40"
            }`}
          >
            <span
              className={`w-4 h-4 rounded-full border-2 shrink-0 flex items-center justify-center transition-colors ${
                active ? "border-teal" : "border-border"
              }`}
            >
              {active && <span className="w-2 h-2 rounded-full bg-teal block" />}
            </span>
            <span className="leading-relaxed">{opt.label}</span>
          </button>
        );
      })}
    </div>
  );
}

const FADE_MS = 220;
const STEP_MS = 180;

export default function ClientRiskAnalyzerPage() {
  const [stage, setStage] = useState<Stage>("intro");
  const [pageVisible, setPageVisible] = useState(true);
  const [currentStep, setCurrentStep] = useState(0);
  const [stepVisible, setStepVisible] = useState(true);
  const [form, setForm] = useState<FormState>(INITIAL);
  const [localResult, setLocalResult] = useState<ReturnType<typeof computeRisk>>(null);
  const [aiAnalysis, setAiAnalysis] = useState<GemmaRiskAnalysis | null>(null);
  const [analysisStatus, setAnalysisStatus] = useState<AnalysisStatus>("idle");
  const [analysisError, setAnalysisError] = useState("");

  const goToStage = (s: Stage) => {
    setPageVisible(false);
    setTimeout(() => { setStage(s); setPageVisible(true); }, FADE_MS);
  };

  const setField = (key: keyof FormState) => (value: string) => {
    setForm((prev) => ({ ...prev, [key]: value }));
    if (currentStep < QUESTIONS.length - 1) {
      setTimeout(() => {
        setStepVisible(false);
        setTimeout(() => {
          setCurrentStep((prev) => prev + 1);
          setStepVisible(true);
        }, STEP_MS);
      }, 380);
    }
  };

  const goBack = () => {
    if (currentStep > 0) {
      setStepVisible(false);
      setTimeout(() => {
        setCurrentStep((prev) => prev - 1);
        setStepVisible(true);
      }, STEP_MS);
    } else {
      goToStage("intro");
      setTimeout(() => { setForm(INITIAL); setCurrentStep(0); }, FADE_MS);
    }
  };

  const handleAnalyze = async () => {
    const r = computeRisk(form);
    if (!r) return;
    setLocalResult(r);
    goToStage("loading");
    setAnalysisStatus("loading");
    try {
      const ai = await requestGemmaAnalysis(form, r);
      setAiAnalysis(ai);
      setAnalysisStatus("success");
    } catch (e) {
      setAnalysisError(e instanceof Error ? e.message : "خطأ غير متوقع");
      setAnalysisStatus("error");
    }
    goToStage("result");
  };

  const handleReset = () => {
    goToStage("intro");
    setTimeout(() => {
      setForm(INITIAL);
      setCurrentStep(0);
      setLocalResult(null);
      setAiAnalysis(null);
      setAnalysisStatus("idle");
      setAnalysisError("");
    }, FADE_MS);
  };

  const q = QUESTIONS[currentStep];
  const isLastStep = currentStep === QUESTIONS.length - 1;
  const canAnalyze = isLastStep && !!form[q?.id];
  const answeredCount = QUESTIONS.filter((q) => !!form[q.id]).length;

  const localConfig = localResult ? RISK_CONFIG[localResult.level] : null;
  const aiConfig = aiAnalysis ? RISK_CONFIG[aiAnalysis.riskLevel] : null;
  const RiskIcon = localConfig?.icon ?? ShieldAlert;
  const scorePercent = localResult ? Math.round((localResult.score / 7) * 100) : 0;

  return (
    <div
      className="mx-auto max-w-2xl"
      style={{
        transition: `opacity ${FADE_MS}ms ease, transform ${FADE_MS}ms ease`,
        opacity: pageVisible ? 1 : 0,
        transform: pageVisible ? "translateY(0)" : "translateY(12px)",
      }}
    >
      {/* ── Intro ── */}
      {stage === "intro" && (
        <div className="text-center space-y-8 py-6">
          <div className="mx-auto w-20 h-20 rounded-2xl bg-gradient-brand grid place-items-center shadow-glow">
            <ShieldAlert className="w-10 h-10 text-white" />
          </div>

          <div>
            <h2 className="text-2xl font-bold text-navy mb-3">تحليل مخاطر العميل</h2>
            <p className="text-muted-foreground text-sm leading-relaxed max-w-md mx-auto">
              قبل أن تبدأ أي مشروع، نساعدك على تقييم مستوى الخطورة بناءً على سلوك العميل وتجاربك السابقة معه — في أقل من دقيقة.
            </p>
          </div>

          <div className="grid grid-cols-3 gap-4 max-w-sm mx-auto text-center">
            {[
              { n: "١", label: "أجب على ٦ أسئلة قصيرة" },
              { n: "٢", label: "نحلل بالذكاء الاصطناعي" },
              { n: "٣", label: "تحصل على تقرير مفصل" },
            ].map((item) => (
              <div key={item.n} className="bg-card border border-border/60 rounded-2xl p-4">
                <div className="w-8 h-8 rounded-full bg-gradient-brand text-white text-sm font-bold grid place-items-center mx-auto mb-2 shadow-glow">
                  {item.n}
                </div>
                <p className="text-xs text-muted-foreground leading-relaxed">{item.label}</p>
              </div>
            ))}
          </div>

          <button
            onClick={() => goToStage("questions")}
            className="inline-flex items-center gap-2 px-8 py-3 rounded-xl bg-gradient-brand text-white font-semibold shadow-glow hover:opacity-90 transition-opacity"
          >
            ابدأ التحليل
            <ArrowLeft className="w-4 h-4" />
          </button>
        </div>
      )}

      {/* ── Questions ── */}
      {stage === "questions" && q && (
        <div className="space-y-6">
          {/* Progress bar */}
          <div>
            <div className="flex justify-between text-xs text-muted-foreground mb-2">
              <span>السؤال {currentStep + 1} من {QUESTIONS.length}</span>
              <span>{Math.round((answeredCount / QUESTIONS.length) * 100)}٪ مكتمل</span>
            </div>
            <div className="h-1.5 rounded-full bg-muted overflow-hidden">
              <div
                className="h-full rounded-full bg-gradient-brand transition-all duration-500"
                style={{ width: `${(answeredCount / QUESTIONS.length) * 100}%` }}
              />
            </div>
          </div>

          {/* Question card */}
          <div
            style={{
              transition: `opacity ${STEP_MS}ms ease, transform ${STEP_MS}ms ease`,
              opacity: stepVisible ? 1 : 0,
              transform: stepVisible ? "translateY(0)" : "translateY(10px)",
            }}
            className="bg-card rounded-2xl p-6 border border-border/70 shadow-card"
          >
            <div className="mb-5 text-center">
              <span className="inline-flex items-center gap-1.5 text-xs text-muted-foreground mb-2">
                <span className="w-5 h-5 rounded-full bg-gradient-brand text-white text-[10px] font-bold grid place-items-center shadow-glow">
                  {currentStep + 1}
                </span>
                {q.label}
              </span>
              <p className="text-sm font-semibold text-navy leading-relaxed">{q.hint}</p>
            </div>

            <RadioGroup
              name={q.id}
              options={q.options}
              value={form[q.id]}
              onChange={setField(q.id)}
            />
          </div>

          {/* Navigation */}
          <div className="flex gap-3">
            <button
              onClick={goBack}
              className="px-5 py-3 rounded-xl border border-border/60 text-navy text-sm font-medium hover:bg-muted/40 transition-colors"
            >
              رجوع
            </button>

            {canAnalyze && (
              <button
                onClick={handleAnalyze}
                className="flex-1 py-3 rounded-xl bg-gradient-brand text-white font-semibold text-sm shadow-glow hover:opacity-90 transition-opacity flex items-center justify-center gap-2"
              >
                <Sparkles className="w-4 h-4" />
                تحليل مستوى الخطورة
              </button>
            )}
          </div>
        </div>
      )}

      {/* ── Loading ── */}
      {stage === "loading" && (
        <div className="text-center py-24 space-y-5">
          <div className="mx-auto w-16 h-16 rounded-2xl bg-gradient-brand grid place-items-center shadow-glow">
            <Loader2 className="w-8 h-8 text-white animate-spin" />
          </div>
          <div>
            <p className="text-base font-semibold text-navy">جاري تحليل بيانات العميل...</p>
            <p className="text-xs text-muted-foreground mt-1">يتم إرسال البيانات إلى Gemma 4 لإعداد التقرير</p>
          </div>
        </div>
      )}

      {/* ── Result ── */}
      {stage === "result" && localResult && localConfig && (
        <div className="space-y-4">
          {analysisStatus === "error" && (
            <>
          {/* Risk level */}
          <div className={`rounded-2xl p-6 border ${localConfig.bg} ${localConfig.border} shadow-card`}>
            <div className="flex items-center gap-3 mb-4">
              <div className={`w-12 h-12 rounded-xl grid place-items-center bg-white/60 ${localConfig.textColor}`}>
                <RiskIcon className="w-6 h-6" />
              </div>
              <div>
                <p className={`font-bold text-base ${localConfig.textColor}`}>{localConfig.label}</p>
                <p className="text-xs text-muted-foreground">{localConfig.sublabel}</p>
              </div>
            </div>
            <div className="mb-1 flex justify-between text-xs">
              <span className="text-muted-foreground">درجة الخطورة</span>
              <span className={`font-bold ${localConfig.textColor}`}>{scorePercent}٪</span>
            </div>
            <div className="h-2 rounded-full bg-white/60 overflow-hidden">
              <div
                className={`h-full rounded-full transition-all duration-700 ${localConfig.barColor}`}
                style={{ width: `${scorePercent}%` }}
              />
            </div>
          </div>

          {/* Risk factors */}
          {localResult.factors.length > 0 && (
            <div className="bg-card rounded-2xl p-5 border border-border/70 shadow-card">
              <h4 className="text-sm font-bold text-navy mb-3">عوامل الخطورة المكتشفة</h4>
              <ul className="space-y-2">
                {localResult.factors.map((f, i) => (
                  <li key={i} className="flex items-start gap-2 text-xs text-navy">
                    <span className="w-1.5 h-1.5 rounded-full bg-danger mt-1.5 shrink-0" />
                    {f}
                  </li>
                ))}
              </ul>
            </div>
          )}

          {/* Recommendations */}
          <div className="bg-card rounded-2xl p-5 border border-border/70 shadow-card">
            <h4 className="text-sm font-bold text-navy mb-3">توصيات مالي</h4>
            <ul className="space-y-2">
              {localConfig.recommendations.map((r, i) => (
                <li key={i} className="flex gap-2.5 p-2.5 rounded-xl bg-muted/40 text-xs text-navy leading-relaxed">
                  <span className="w-5 h-5 rounded-full bg-teal text-white grid place-items-center text-[10px] font-bold shrink-0">
                    {i + 1}
                  </span>
                  {r}
                </li>
              ))}
            </ul>
          </div>
            </>
          )}

          {/* AI analysis */}
          <div className="bg-card rounded-2xl p-5 border border-border/70 shadow-card">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-10 h-10 rounded-xl bg-gradient-brand text-white grid place-items-center shadow-glow">
                <Bot className="w-5 h-5" />
              </div>
              <div>
                <h4 className="text-sm font-bold text-navy">تحليل Gemma 4 الاحترافي</h4>
                <p className="text-xs text-muted-foreground">{GEMMA_MODEL}</p>
              </div>
            </div>

            {analysisStatus === "error" && (
              <div className="rounded-xl border border-warning/40 bg-warning-soft p-4 text-xs text-navy">
                <p className="font-semibold mb-1">تعذر عرض تحليل Gemma 4.</p>
                <p className="text-muted-foreground">{analysisError}</p>
              </div>
            )}

            {analysisStatus === "success" && aiAnalysis && aiConfig && (
              <div className="space-y-4">
                <div className={`rounded-xl p-4 border ${aiConfig.bg} ${aiConfig.border}`}>
                  <div className="flex items-center justify-between gap-3 mb-3">
                    <div>
                      <p className={`text-sm font-bold ${aiConfig.textColor}`}>{aiConfig.label}</p>
                      <p className="text-xs text-muted-foreground mt-1">{aiAnalysis.executiveSummary}</p>
                    </div>
                    <Sparkles className={`w-5 h-5 shrink-0 ${aiConfig.textColor}`} />
                  </div>
                  <div className="grid grid-cols-2 gap-3">
                    <div className="rounded-lg bg-white/60 p-3">
                      <p className="text-[11px] text-muted-foreground">درجة المخاطر</p>
                      <p className={`text-lg font-bold ${aiConfig.textColor}`}>{aiAnalysis.riskScore}%</p>
                    </div>
                    <div className="rounded-lg bg-white/60 p-3">
                      <p className="text-[11px] text-muted-foreground">الثقة</p>
                      <p className="text-lg font-bold text-navy">{aiAnalysis.confidence}%</p>
                    </div>
                  </div>
                </div>

                {[
                  { title: "أبرز المخاطر", items: aiAnalysis.keyRisks },
                  { title: "توصيات عملية", items: aiAnalysis.recommendations },
                  { title: "بنود تعاقدية مقترحة", items: aiAnalysis.contractTerms },
                  { title: "الخطوات التالية", items: aiAnalysis.nextSteps },
                ].map((section) =>
                  section.items.length > 0 ? (
                    <div key={section.title}>
                      <h5 className="text-xs font-bold text-navy mb-2">{section.title}</h5>
                      <ul className="space-y-2">
                        {section.items.map((item, i) => (
                          <li key={i} className="flex gap-2.5 p-2.5 rounded-xl bg-muted/40 text-xs text-navy leading-relaxed">
                            <span className="w-5 h-5 rounded-full bg-teal text-white grid place-items-center text-[10px] font-bold shrink-0">
                              {i + 1}
                            </span>
                            {item}
                          </li>
                        ))}
                      </ul>
                    </div>
                  ) : null,
                )}
              </div>
            )}
          </div>

          {/* Reset */}
          <button
            onClick={handleReset}
            className="w-full py-3 rounded-xl border border-border/60 text-navy text-sm font-medium hover:bg-muted/40 transition-colors flex items-center justify-center gap-2"
          >
            <RotateCcw className="w-4 h-4" />
            تحليل عميل جديد
          </button>
        </div>
      )}
    </div>
  );
}
