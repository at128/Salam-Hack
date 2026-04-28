import RevenueChart from "@/components/dashboard/RevenueChart";
import TransactionsList from "@/components/dashboard/TransactionsList";
import { PageHeader } from "@/components/dashboard/DashboardLayout";

export default function CashflowPage() {
  return (
    <>
      <PageHeader title="التدفق النقدي" desc="متى يدخل المال ومتى يخرج — وخطّط لشهرك القادم بثقة." />
      <RevenueChart />
      <TransactionsList />
    </>
  );
}