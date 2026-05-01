import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import {
  AlertTriangle,
  BriefcaseBusiness,
  ClipboardList,
  FilePlus2,
  FileText,
  FolderPlus,
  Lightbulb,
  Percent,
  Plus,
  ReceiptText,
  Shield,
  ShieldCheck,
  Siren,
  UserPlus,
  type LucideIcon,
} from "lucide-react";
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

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const ANALYSIS_DASHBOARD_URL = `${API_BASE_URL}/api/v1/analysis/dashboard`;

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
    throw new Error(getApiErrorMessage(payload, "Unable to load dashboard statistics."));
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

type QuickAction = {
  title: string;
  description: string;
  href: string;
  icon: LucideIcon;
  featured?: boolean;
};

const quickActions: QuickAction[] = [
  {
    title: "مشروع جديد",
    description: "ابدأ من السعر والعميل والخدمة",
    href: "/dashboard/projects",
    icon: Plus,
    featured: true,
  },
  {
    title: "فاتورة",
    description: "أنشئ فاتورة أو سجل دفعة",
    href: "/dashboard/invoices",
    icon: FilePlus2,
  },
  {
    title: "مصروف",
    description: "اشتراك، أداة، أو تكلفة عامة",
    href: "/dashboard/profit",
    icon: ReceiptText,
  },
  {
    title: "عميل",
    description: "أضف عميل قبل إنشاء المشروع",
    href: "/dashboard/customers",
    icon: UserPlus,
  },
];

const priorityItems = [
  {
    title: "ابدأ بمشروعك الأول",
    description: "المشروع هو نقطة الربط بين العميل، الفاتورة، والربح",
    href: "/dashboard/projects",
    icon: FolderPlus,
  },
  {
    title: "سجل مصروف جديد",
    description: "الاشتراكات والأدوات تظهر تأثيرها في الربح الحقيقي",
    href: "/dashboard/profit",
    icon: ClipboardList,
  },
  {
    title: "راجع الفواتير والتحصيل",
    description: "الفواتير والدفعات صارت في صفحة واحدة",
    href: "/dashboard/invoices",
    icon: FileText,
  },
];

function MetricCard({
  label,
  value,
  delta,
  trend,
  icon: Icon,
}: {
  label: string;
  value: string;
  delta: string;
  trend: "up" | "down" | "neutral";
  icon: LucideIcon;
}) {
  const tone =
    trend === "up"
      ? "bg-success-soft text-success"
      : trend === "down"
        ? "bg-warning-soft text-warning"
        : "bg-muted text-muted-foreground";

  return (
    <article className="min-h-36 rounded-2xl border border-border/70 bg-card p-5 shadow-card">
      <div className="flex items-start justify-between gap-3">
        <span className={`inline-flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold ${tone}`}>
          {delta}
        </span>
        <span className="grid h-11 w-11 place-items-center rounded-xl bg-teal-soft text-teal">
          <Icon className="h-5 w-5" />
        </span>
      </div>
      <div className="mt-6 text-2xl font-extrabold text-navy">{value}</div>
      <div className="mt-1 text-xs text-muted-foreground">{label}</div>
    </article>
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
        setStatsError(error instanceof Error ? error.message : "Unable to load dashboard statistics.");
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
        label: "مشاريع معرضة للخطر",
        value: isLoadingStats ? "..." : formatNumber(dashboard?.atRiskCount),
        delta: "معرضة للخطر",
        positive: false,
        trend: "down" as const,
        icon: AlertTriangle,
      },
      {
        label: "مشاريع حرجة",
        value: isLoadingStats ? "..." : formatNumber(dashboard?.criticalCount),
        delta: "حرجة",
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

      <section className="grid gap-4 xl:grid-cols-[1.95fr_1fr]">
        <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
          <div className="mb-5 flex items-start justify-between gap-4">
            <div className="text-right">
              <p className="mb-1 text-xs font-semibold text-teal">الصفحات الأساسية</p>
              <h2 className="text-2xl font-extrabold tracking-normal text-navy">إدارة الشغل من مكان واحد</h2>
            </div>
            <Link
              to="/dashboard/client-risk"
              className="inline-flex shrink-0 items-center gap-2 rounded-xl border border-border/70 bg-background px-3 py-2 text-sm font-semibold text-navy transition-colors hover:bg-muted/60"
            >
              <Shield className="h-4 w-4" />
              تحليل عميل
            </Link>
          </div>

          <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            {quickActions.map((action) => {
              const Icon = action.icon;
              return (
                <Link
                  key={action.title}
                  to={action.href}
                  className={
                    action.featured
                      ? "group flex min-h-36 flex-col justify-between rounded-2xl bg-navy p-4 text-white shadow-card transition-transform hover:-translate-y-0.5"
                      : "group flex min-h-36 flex-col justify-between rounded-2xl border border-border/70 bg-background p-4 transition-colors hover:bg-muted/40"
                  }
                >
                  <div className="flex items-center justify-between gap-3">
                    <span className={action.featured ? "text-xs text-white/70" : "text-xs text-muted-foreground"}>
                      فتح
                    </span>
                    <span
                      className={
                        action.featured
                          ? "grid h-10 w-10 place-items-center rounded-xl bg-white/10 text-white"
                          : "grid h-10 w-10 place-items-center rounded-xl bg-teal-soft text-teal"
                      }
                    >
                      <Icon className="h-5 w-5" />
                    </span>
                  </div>
                  <div>
                    <h3 className={action.featured ? "text-lg font-extrabold text-white" : "text-lg font-extrabold text-navy"}>
                      {action.title}
                    </h3>
                    <p className={action.featured ? "mt-1 text-xs leading-relaxed text-white/70" : "mt-1 text-xs leading-relaxed text-muted-foreground"}>
                      {action.description}
                    </p>
                  </div>
                </Link>
              );
            })}
          </div>
        </div>

        <aside className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
          <div className="mb-5 text-right">
            <h2 className="text-xl font-extrabold text-navy">الأولوية الآن</h2>
            <p className="mt-1 text-xs text-muted-foreground">مختصر عملي بدل التنقل بين تقارير كثيرة.</p>
          </div>

          <div className="space-y-3">
            {priorityItems.map((item) => {
              const Icon = item.icon;
              return (
                <Link
                  key={item.title}
                  to={item.href}
                  className="flex items-center justify-between gap-3 rounded-2xl border border-border/70 bg-background p-4 transition-colors hover:bg-muted/40"
                >
                  <span className="grid h-10 w-10 shrink-0 place-items-center rounded-xl bg-teal-soft text-teal">
                    <Icon className="h-4 w-4" />
                  </span>
                  <span className="min-w-0 flex-1 text-right">
                    <span className="block font-extrabold text-navy">{item.title}</span>
                    <span className="mt-1 block text-xs leading-relaxed text-muted-foreground">{item.description}</span>
                  </span>
                </Link>
              );
            })}
          </div>
        </aside>
      </section>

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-5">
        {kpis.map((k) => (
          <MetricCard key={k.label} {...k} />
        ))}
      </section>

      <section className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2 rounded-2xl border border-border/70 bg-card p-5 shadow-card">
          <div className="mb-4 flex items-center justify-between">
            <div>
              <h2 className="text-lg font-bold text-navy">تحليلات هذا الشهر</h2>
              <p className="text-xs text-muted-foreground">النتائج القادمة مباشرة من تحليل المشاريع.</p>
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
                    <span className="shrink-0 rounded-full bg-background/60 px-2 py-1 text-xs">{insightTypeLabel(insight.type)}</span>
                  </div>
                  <p className="text-sm leading-relaxed">{insight.summary}</p>
                </article>
              ))}
            </div>
          ) : (
            <div className="rounded-xl bg-muted/50 p-6 text-sm text-muted-foreground">
              لا توجد تحليلات شهرية بعد.
            </div>
          )}
        </div>

        <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
          <h2 className="mb-4 text-lg font-bold text-navy">المشروع المحدد</h2>
          {isLoadingStats ? (
            <div className="rounded-xl bg-muted/50 p-6 text-sm text-muted-foreground">جاري تحميل المشروع...</div>
          ) : selectedProject ? (
            <div className="space-y-4">
              <div>
                <h3 className="font-bold text-navy">{selectedProject.projectName}</h3>
                <p className="text-xs text-muted-foreground">{selectedProject.customerName} - {selectedProject.serviceName}</p>
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
        <div className="mb-4">
          <h2 className="text-lg font-bold text-navy">صحة المشاريع</h2>
          <p className="text-xs text-muted-foreground">قائمة المشاريع كما ترجعها واجهة التحليل.</p>
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
