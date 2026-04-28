import KpiCard from "@/components/dashboard/KpiCard";
import RevenueChart from "@/components/dashboard/RevenueChart";
import AlertsPanel from "@/components/dashboard/AlertsPanel";
import TransactionsList from "@/components/dashboard/TransactionsList";
import InvoicesTable from "@/components/dashboard/InvoicesTable";
import ServiceProfits from "@/components/dashboard/ServiceProfits";
import { KPIS } from "@/data/mali";

export default function Dashboard() {
  return (
    <>
      <section className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {KPIS.map((k) => (
          <KpiCard key={k.label} {...k} />
        ))}
      </section>

      <section className="grid lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2">
          <RevenueChart />
        </div>
        <AlertsPanel />
      </section>

      <section className="grid lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2">
          <InvoicesTable />
        </div>
        <TransactionsList />
      </section>

      <section>
        <ServiceProfits />
      </section>
    </>
  );
}