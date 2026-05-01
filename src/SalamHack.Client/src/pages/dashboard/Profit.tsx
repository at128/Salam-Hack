import { useEffect, useState } from "react";
import KpiCard from "@/components/dashboard/KpiCard";
import RevenueChart from "@/components/dashboard/RevenueChart";
import { PageHeader } from "@/components/dashboard/DashboardLayout";
import { fetchProfitabilityReport, ProfitabilityReportDto } from "@/lib/reports";
import { Wallet, TrendingUp, Calculator } from "lucide-react";

export default function ProfitPage() {
  const [report, setReport] = useState<ProfitabilityReportDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    fetchProfitabilityReport()
      .then(setReport)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return <div className="p-8 text-center text-muted-foreground animate-pulse">جاري تحميل البيانات...</div>;
  }

  if (error) {
    return <div className="p-8 text-center text-destructive">{error}</div>;
  }

  const kpis = report ? [
    { label: "إجمالي الإيرادات", value: `${report.summary.totalRevenue} ر.س`, delta: "", positive: true, icon: Wallet },
    { label: "صافي الربح", value: `${report.summary.totalProfit} ر.س`, delta: `${report.summary.overallMarginPercent}% هامش`, positive: report.summary.totalProfit >= 0, icon: TrendingUp },
    { label: "إجمالي المصاريف", value: `${report.summary.totalExpenses} ر.س`, delta: "", positive: false, icon: Calculator },
  ] : [];

  const chartData = report?.monthlyTrend.map(t => {
    const months = ["يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"];
    return {
      monthName: months[t.month - 1] + " " + String(t.year).slice(-2),
      revenue: t.revenue,
      expenses: t.expenses
    };
  }) || [];

  return (
    <>
      <PageHeader title="كشف الربح الحقيقي" desc="ربحك الصافي بعد كل المصاريف والاشتراكات." />
      <section className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {kpis.map((k) => (
          <KpiCard key={k.label} {...k} />
        ))}
      </section>
      {chartData.length > 0 && <div className="mt-8"><RevenueChart data={chartData} /></div>}
    </>
  );
}