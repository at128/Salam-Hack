import ServiceProfits from "@/components/dashboard/ServiceProfits";
import { PageHeader } from "@/components/dashboard/DashboardLayout";

export default function BreakdownPage() {
  return (
    <>
      <PageHeader title="أين ذهب ربحك؟" desc="تحليل مفصّل لربحية كل خدمة تقدمها." />
      <ServiceProfits />
    </>
  );
}