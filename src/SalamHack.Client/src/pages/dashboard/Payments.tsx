import TransactionsList from "@/components/dashboard/TransactionsList";
import { PageHeader } from "@/components/dashboard/DashboardLayout";

export default function PaymentsPage() {
  return (
    <>
      <PageHeader title="المدفوعات" desc="حركة الدخل والمصاريف لحظة بلحظة." />
      <div className="grid lg:grid-cols-2 gap-6">
        <TransactionsList />
        <TransactionsList />
      </div>
    </>
  );
}