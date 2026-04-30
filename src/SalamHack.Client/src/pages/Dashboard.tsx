import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import {
  AlertTriangle,
  BriefcaseBusiness,
  FileText,
  FolderKanban,
  Lightbulb,
  Percent,
  Plus,
  ReceiptText,
  ShieldAlert,
  ShieldCheck,
  Siren,
  Sparkles,
  Users,
  type LucideIcon,
} from "lucide-react";
import KpiCard from "@/components/dashboard/KpiCard";
import { getApiErrorMessage, getValidAccessToken, unwrapApiResponse } from "@/lib/auth";

type AnalysisInsight = {
  type: string;
  severity: string;
  title: string;
  summary: string;
};

type ProjectAnalysisListItem = {
  projectId: string;
  projectName: string;
  customerId: string;
  customerName: string;
  marginPercent: number;
  healthStatus: string;
  profit: number;
};

type ProjectAnalysis = {
  projectId: string;
  projectName: string;
  customerName: string;
  serviceName: string;
  healthStatus: string;
  whatHappened: string;
  whatItMeans: string;
  whatToDo: string;
};

type AnalysisDashboard = {
  projectCount: number;
  healthyCount: number;
  atRiskCount: number;
  criticalCount: number;
  averageMarginPercent: number;
  monthlyInsights: AnalysisInsight[];
  projects: ProjectAnalysisListItem[];
  selectedProject: ProjectAnalysis | null;
};

type ActionLink = {
  title: string;
  desc: string;
  href: string;
  icon: LucideIcon;
  primary?: boolean;
};

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const ANALYSIS_DASHBOARD_URL = `${API_BASE_URL}/api/v1/analysis/dashboard`;

const quickActions: ActionLink[] = [
  {
    title: "مشروع جديد",
    desc: "ابدأ من السعر والعميل والخدمة",
    href: "/dashboard/projects",
    icon: Plus,
    primary: true,
  },
  {
    title: "فاتورة",
    desc: "أنشئ فاتورة أو سجل دفعة",
    href: "/dashboard/invoices",
    icon: FileText,
  },
  {
    title: "مصروف",
    desc: "اشتراك، أداة، أو تكلفة عامة",
    href: "/dashboard/expenses",
    icon: ReceiptText,
  },
  {
    title: "عميل",
    desc: "أضف عميل قبل إنشاء المشروع",
    href: "/dashboard/customers",
    icon: Users,
  },
];

async function fetchAnalysisDashboard(): Promise<AnalysisDashboard> {
  const token = await getValidAccessToken();
  if (!token) throw new Error("Missing access token.");

  const response = await fetch(ANALYSIS_DASHBOARD_URL, {
    method: "GET",
    headers: {
      Accept: "application/json",
      Authorization: `Bearer ${token}`,
    },
  });

  const payload = await response.json().catch(() => null);

  if (!response.ok) {
    throw new Error(getApiErrorMessage(payload, "تعذر تحميل ملخص لوحة التحكم."));
  }

  return unwrapApiResponse<AnalysisDashboard>(payload);
}

function formatNumber(value: number | undefined) {
  return new Intl.NumberFormat("ar").format(value ?? 0);
}

function formatPercent(value: number | undefined) {
  return `${new Intl.NumberFormat("ar", { maximumFractionDigits: 1 }).format(value ?? 0)}%`;
}

function formatCurrency(value: number | undefined) {
  return new Intl.NumberFormat("ar", {
    style: "currency",
    currency: "SAR",
    maximumFractionDigits: 0,
  }).format(value ?? 0);
}

function severityClass(severity: string | undefined) {
  switch (severity?.toLowerCase()) {
    case "success":
      return "border-success/30 bg-success-soft text-success";
    case "warning":
      return "border-warning/30 bg-warning-soft text-warning";
    case "critical":
      return "border-danger/30 bg-danger-soft text-danger";
    default:
      return "border-border bg-muted/50 text-muted-foreground";
  }
}

function healthClass(status: string | undefined) {
  switch (status?.toLowerCase()) {
    case "healthy":
      return "bg-success-soft text-success";
    case "atrisk":
    case "at risk":
      return "bg-warning-soft text-warning";
    case "critical":
      return "bg-danger-soft text-danger";
    default:
      return "bg-muted text-muted-foreground";
  }
}

function healthLabel(status: string | undefined) {
  switch (status?.toLowerCase()) {
    case "healthy":
      return "صحي";
    case "atrisk":
    case "at risk":
      return "معرض للخطر";
    case "critical":
      return "حرج";
    default:
      return status ?? "-";
  }
}

function insightTypeLabel(type: string | undefined) {
  switch (type?.toLowerCase()) {
    case "projecthealth":
      return "صحة المشروع";
    case "generalinsight":
      return "رؤية عامة";
    case "expensetrend":
      return "اتجاه المصاريف";
    default:
      return type ?? "-";
  }
}

function QuickAction({ action }: { action: ActionLink }) {
  const Icon = action.icon;

  return (
    <Link
      to={action.href}
      className={`rounded-xl border p-4 transition-colors ${
        action.primary
          ? "border-navy bg-navy text-white hover:bg-navy-light"
          : "border-border/70 bg-card text-navy hover:border-navy/25 hover:bg-muted/30"
      }`}
    >
      <div className="mb-3 flex items-center justify-between">
        <span className={`grid h-10 w-10 place-items-center rounded-xl ${action.primary ? "bg-white/10" : "bg-teal-soft text-teal"}`}>
          <Icon className="h-5 w-5" />
        </span>
        <span className={`text-xs font-semibold ${action.primary ? "text-white/65" : "text-muted-foreground"}`}>فتح</span>
      </div>
      <h3 className="font-bold">{action.title}</h3>
      <p className={`mt-1 text-xs leading-relaxed ${action.primary ? "text-white/65" : "text-muted-foreground"}`}>
        {action.desc}
      </p>
    </Link>
  );
}

function PriorityStep({ step }: { step: ActionLink }) {
  const Icon = step.icon;

  return (
    <Link
      to={step.href}
      className="flex items-start gap-3 rounded-xl border border-border/70 bg-card p-3 transition-colors hover:border-navy/25 hover:bg-muted/30"
    >
      <span className="grid h-9 w-9 shrink-0 place-items-center rounded-xl bg-teal-soft text-teal">
        <Icon className="h-4 w-4" />
      </span>
      <span className="min-w-0">
        <span className="block font-semibold text-navy">{step.title}</span>
        <span className="mt-0.5 block text-xs leading-relaxed text-muted-foreground">{step.desc}</span>
      </span>
    </Link>
  );
}

export default function Dashboard() {
  const [dashboard, setDashboard] = useState<AnalysisDashboard | null>(null);
  const [isLoadingStats, setIsLoadingStats] = useState(true);
  const [statsError, setStatsError] = useState("");

  useEffect(() => {
    let active = true;
    setIsLoadingStats(true);
    setStatsError("");

    fetchAnalysisDashboard()
      .then((result) => {
        if (!active) return;
        setDashboard(result);
      })
      .catch((error) => {
        if (!active) return;
        setStatsError(error instanceof Error ? error.message : "تعذر تحميل ملخص لوحة التحكم.");
      })
      .finally(() => {
        if (!active) return;
        setIsLoadingStats(false);
      });

    return () => {
      active = false;
    };
  }, []);

  const kpis = useMemo(
    () => [
      {
        label: "إجمالي المشاريع",
        value: isLoadingStats ? "..." : formatNumber(dashboard?.projectCount),
        delta: "كل المشاريع",
        positive: true,
        trend: "neutral" as const,
        icon: BriefcaseBusiness,
      },
      {
        label: "مشاريع صحية",
        value: isLoadingStats ? "..." : formatNumber(dashboard?.healthyCount),
        delta: "صحية",
        positive: true,
        trend: "up" as const,
        icon: ShieldCheck,
      },
      {
        label: "تحتاج متابعة",
        value: isLoadingStats ? "..." : formatNumber(dashboard?.atRiskCount),
        delta: "معرضة للخطر",
        positive: false,
        trend: "down" as const,
        icon: AlertTriangle,
      },
      {
        label: "حرجة",
        value: isLoadingStats ? "..." : formatNumber(dashboard?.criticalCount),
        delta: "عاجلة",
        positive: false,
        trend: "down" as const,
        icon: Siren,
      },
      {
        label: "متوسط هامش الربح",
        value: isLoadingStats ? "..." : formatPercent(dashboard?.averageMarginPercent),
        delta: "متوسط",
        positive: true,
        trend: "neutral" as const,
        icon: Percent,
      },
    ],
    [isLoadingStats, dashboard],
  );

  const prioritySteps = useMemo<ActionLink[]>(() => {
    if (isLoadingStats) {
      return [
        {
          title: "تحميل الأولويات",
          desc: "نجهز ملخص اليوم من بياناتك",
          href: "/dashboard",
          icon: Sparkles,
        },
      ];
    }

    const steps: ActionLink[] = [];

    if ((dashboard?.projectCount ?? 0) === 0) {
      steps.push({
        title: "ابدأ بمشروعك الأول",
        desc: "المشروع هو نقطة الربط بين العميل، الفاتورة، والربح",
        href: "/dashboard/projects",
        icon: FolderKanban,
      });
    }

    if ((dashboard?.criticalCount ?? 0) > 0) {
      steps.push({
        title: "راجع المشاريع الحرجة",
        desc: "فيه مشاريع ممكن تأكل من ربحك لو تركتها",
        href: "/dashboard/projects",
        icon: Siren,
      });
    }

    if ((dashboard?.atRiskCount ?? 0) > 0) {
      steps.push({
        title: "تابع المشاريع المعرضة للخطر",
        desc: "راجع السعر، الساعات، أو المصاريف قبل ما تتراكم",
        href: "/dashboard/projects",
        icon: ShieldAlert,
      });
    }

    steps.push(
      {
        title: "سجل مصروف جديد",
        desc: "الاشتراكات والأدوات لازم تظهر في الربح الحقيقي",
        href: "/dashboard/expenses",
        icon: ReceiptText,
      },
      {
        title: "راجع الفواتير والتحصيل",
        desc: "الفواتير والدفعات صارت في صفحة واحدة",
        href: "/dashboard/invoices",
        icon: FileText,
      },
    );

    return steps.slice(0, 3);
  }, [dashboard, isLoadingStats]);

  const insights = dashboard?.monthlyInsights ?? [];
  const projects = dashboard?.projects ?? [];
  const selectedProject = dashboard?.selectedProject ?? null;

  return (
    <>
      {statsError && (
        <div className="rounded-xl border border-danger/30 bg-danger-soft p-3 text-sm text-danger">
          {statsError}
        </div>
      )}

      <section className="grid gap-4 xl:grid-cols-[1.45fr_0.75fr]">
        <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
          <div className="mb-5 flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <p className="text-xs font-semibold text-teal">الصفحات الأساسية</p>
              <h2 className="mt-1 text-xl font-bold text-navy">إدارة الشغل من مكان واحد</h2>
            </div>
            <Link
              to="/dashboard/client-risk"
              className="inline-flex items-center justify-center gap-2 rounded-xl border border-border/70 px-3 py-2 text-sm font-semibold text-navy transition-colors hover:bg-muted/40"
            >
              <ShieldAlert className="h-4 w-4" />
              تحليل عميل
            </Link>
          </div>

          <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            {quickActions.map((action) => (
              <QuickAction key={action.href} action={action} />
            ))}
          </div>
        </div>

        <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
          <div className="mb-4">
            <h2 className="text-lg font-bold text-navy">الأولوية الآن</h2>
            <p className="mt-1 text-xs text-muted-foreground">مختصر عملي بدل التنقل بين تقارير كثيرة.</p>
          </div>
          <div className="space-y-2">
            {prioritySteps.map((step) => (
              <PriorityStep key={`${step.href}-${step.title}`} step={step} />
            ))}
          </div>
        </div>
      </section>

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-5">
        {kpis.map((k) => (
          <KpiCard key={k.label} {...k} />
        ))}
      </section>

      <section className="grid gap-6 lg:grid-cols-3">
        <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-card lg:col-span-2">
          <div className="mb-4 flex items-center justify-between">
            <div>
              <h2 className="text-lg font-bold text-navy">قرارات هذا الشهر</h2>
              <p className="text-xs text-muted-foreground">تنبيهات مبنية على صحة المشاريع والمصاريف.</p>
            </div>
            <Lightbulb className="h-5 w-5 text-teal" />
          </div>

          {isLoadingStats ? (
            <div className="rounded-xl bg-muted/50 p-6 text-sm text-muted-foreground">جاري تحميل التحليلات...</div>
          ) : insights.length > 0 ? (
            <div className="grid gap-3">
              {insights.map((insight, index) => (
                <article
                  key={`${insight.type}-${insight.title}-${index}`}
                  className={`rounded-xl border p-4 ${severityClass(insight.severity)}`}
                >
                  <div className="mb-1 flex items-center justify-between gap-3">
                    <h3 className="font-bold">{insight.title}</h3>
                    <span className="shrink-0 rounded-full bg-background/60 px-2 py-1 text-xs">
                      {insightTypeLabel(insight.type)}
                    </span>
                  </div>
                  <p className="text-sm leading-relaxed">{insight.summary}</p>
                </article>
              ))}
            </div>
          ) : (
            <div className="rounded-xl bg-muted/50 p-6 text-sm text-muted-foreground">
              لا توجد تحليلات بعد. أضف مشروعاً وفاتورة ومصروفاً حتى تظهر قرارات أوضح.
            </div>
          )}
        </div>

        <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
          <h2 className="mb-4 text-lg font-bold text-navy">مشروع يحتاج انتباه</h2>
          {isLoadingStats ? (
            <div className="rounded-xl bg-muted/50 p-6 text-sm text-muted-foreground">جاري تحميل المشروع...</div>
          ) : selectedProject ? (
            <div className="space-y-4">
              <div>
                <h3 className="font-bold text-navy">{selectedProject.projectName}</h3>
                <p className="text-xs text-muted-foreground">
                  {selectedProject.customerName} - {selectedProject.serviceName}
                </p>
              </div>
              <span className={`inline-flex rounded-full px-2 py-1 text-xs font-bold ${healthClass(selectedProject.healthStatus)}`}>
                {healthLabel(selectedProject.healthStatus)}
              </span>
              <div className="space-y-3 text-sm leading-relaxed text-muted-foreground">
                <p>{selectedProject.whatHappened}</p>
                <p>{selectedProject.whatItMeans}</p>
                <p>{selectedProject.whatToDo}</p>
              </div>
            </div>
          ) : (
            <div className="rounded-xl bg-muted/50 p-6 text-sm text-muted-foreground">
              لا يوجد مشروع محدد للعرض.
            </div>
          )}
        </div>
      </section>

      <section className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
        <div className="mb-4 flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <h2 className="text-lg font-bold text-navy">صحة المشاريع</h2>
            <p className="text-xs text-muted-foreground">أقرب قائمة تحتاج مراجعة قبل ما تتحول لخسارة.</p>
          </div>
          <Link
            to="/dashboard/projects"
            className="text-sm font-semibold text-teal transition-colors hover:text-navy"
          >
            فتح المشاريع
          </Link>
        </div>

        {isLoadingStats ? (
          <div className="rounded-xl bg-muted/50 p-6 text-sm text-muted-foreground">جاري تحميل المشاريع...</div>
        ) : projects.length > 0 ? (
          <div className="overflow-hidden rounded-xl border border-border/70">
            <table className="w-full text-right text-sm">
              <thead className="bg-muted/50 text-xs text-muted-foreground">
                <tr>
                  <th className="px-4 py-3 font-semibold">المشروع</th>
                  <th className="px-4 py-3 font-semibold">العميل</th>
                  <th className="px-4 py-3 font-semibold">هامش الربح</th>
                  <th className="px-4 py-3 font-semibold">الربح</th>
                  <th className="px-4 py-3 font-semibold">الحالة</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border/70">
                {projects.map((project) => (
                  <tr key={project.projectId} className="bg-card">
                    <td className="px-4 py-3 font-semibold text-navy">{project.projectName}</td>
                    <td className="px-4 py-3 text-muted-foreground">{project.customerName}</td>
                    <td className="px-4 py-3 text-muted-foreground">{formatPercent(project.marginPercent)}</td>
                    <td className="px-4 py-3 text-muted-foreground">{formatCurrency(project.profit)}</td>
                    <td className="px-4 py-3">
                      <span className={`inline-flex rounded-full px-2 py-1 text-xs font-bold ${healthClass(project.healthStatus)}`}>
                        {healthLabel(project.healthStatus)}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="rounded-xl bg-muted/50 p-6 text-sm text-muted-foreground">
            لا توجد مشاريع حتى الآن.
          </div>
        )}
      </section>
    </>
  );
}
