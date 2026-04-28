import KpiCard from "@/components/dashboard/KpiCard";
import RevenueChart from "@/components/dashboard/RevenueChart";
import { PageHeader } from "@/components/dashboard/DashboardLayout";
import { KPIS } from "@/data/mali";

export default function ProfitPage() {
  return (
    <>
      <PageHeader title="كشف الربح الحقيقي" desc="ربحك الصافي بعد كل المصاريف والاشتراكات." />
      <section className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {KPIS.map((k) => (
          <KpiCard key={k.label} {...k} />
        ))}
      </section>
      <RevenueChart />
    </>
  );
}