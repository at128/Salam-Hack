import InvoicesTable from "@/components/dashboard/InvoicesTable";
import { PageHeader } from "@/components/dashboard/DashboardLayout";

export default function InvoicesPage() {
  return (
    <>
      <PageHeader title="الفواتير" desc="جميع فواتيرك مع حالاتها وتفاصيل العملاء." />
      <InvoicesTable />
    </>
  );
}