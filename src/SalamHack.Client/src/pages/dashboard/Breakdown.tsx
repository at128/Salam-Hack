import { useState } from "react";
import { CalendarDays } from "lucide-react";
import ServiceProfits from "@/components/dashboard/ServiceProfits";
import CustomerProfits from "@/components/dashboard/CustomerProfits";
import ProjectProfits from "@/components/dashboard/ProjectProfits";
import ProfitabilityInsight from "@/components/dashboard/ProfitabilityInsight";
import { PageHeader } from "@/components/dashboard/DashboardLayout";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

type BreakdownType = "services" | "customers" | "projects";

export default function BreakdownPage() {
  const [activeTab, setActiveTab] = useState<BreakdownType>("services");
  const [dateRange, setDateRange] = useState<"month" | "quarter" | "year" | "custom">("year");
  const [fromUtc, setFromUtc] = useState<string>();
  const [toUtc, setToUtc] = useState<string>();

  const getDates = (range: string) => {
    const now = new Date();
    let from, to;

    to = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59, 999);

    switch (range) {
      case "month":
        from = new Date(now.getFullYear(), now.getMonth(), 1, 0, 0, 0, 0);
        break;
      case "quarter":
        const quarter = Math.floor(now.getMonth() / 3);
        from = new Date(now.getFullYear(), quarter * 3, 1, 0, 0, 0, 0);
        break;
      case "year":
      default:
        from = new Date(now.getFullYear(), 0, 1, 0, 0, 0, 0);
    }

    return { from: from.toISOString(), to: to.toISOString() };
  };

  const handleDateRangeChange = (range: string) => {
    setDateRange(range as any);
    if (range !== "custom") {
      const dates = getDates(range);
      setFromUtc(dates.from);
      setToUtc(dates.to);
    }
  };

  const tabs: { id: BreakdownType; label: string }[] = [
    { id: "services", label: "حسب الخدمة" },
    { id: "customers", label: "حسب العميل" },
    { id: "projects", label: "حسب المشروع" },
  ];

  return (
    <>
      <PageHeader
        title="أين ذهب ربحك؟"
        desc="تحليل مفصّل لربحية كل خدمة تقدمها، عملائك، ومشاريعك."
      />

      {/* Filters */}
      <div className="mb-6 flex flex-col gap-3 rounded-xl border border-border/70 bg-card p-4 sm:flex-row sm:items-center sm:gap-4">
        <CalendarDays className="h-5 w-5 text-muted-foreground" />
        <Select value={dateRange} onValueChange={handleDateRangeChange}>
          <SelectTrigger className="w-full sm:w-[150px]">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="month">هذا الشهر</SelectItem>
            <SelectItem value="quarter">هذا الربع</SelectItem>
            <SelectItem value="year">هذه السنة</SelectItem>
            <SelectItem value="custom">مخصص</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Tabs */}
      <div className="mb-8 flex gap-2 overflow-x-auto border-b border-border/70">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`relative shrink-0 px-4 py-3 font-medium transition-colors ${
              activeTab === tab.id
                ? "text-brand"
                : "text-muted-foreground hover:text-foreground"
            }`}
          >
            {tab.label}
            {activeTab === tab.id && (
              <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-brand" />
            )}
          </button>
        ))}
      </div>

      {/* Content */}
      <div className="space-y-6">
        {/* Insight */}
        <ProfitabilityInsight fromUtc={fromUtc} toUtc={toUtc} />

        {/* Breakdown Data */}
        {activeTab === "services" && (
          <ServiceProfits fromUtc={fromUtc} toUtc={toUtc} />
        )}
        {activeTab === "customers" && (
          <CustomerProfits fromUtc={fromUtc} toUtc={toUtc} />
        )}
        {activeTab === "projects" && (
          <ProjectProfits fromUtc={fromUtc} toUtc={toUtc} />
        )}
      </div>
    </>
  );
}
