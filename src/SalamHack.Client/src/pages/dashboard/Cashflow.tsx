import { useState } from "react";
import { CalendarDays, Settings } from "lucide-react";
import CashFlowSummary from "@/components/dashboard/CashFlowSummary";
import CashFlowProjection from "@/components/dashboard/CashFlowProjection";
import PendingInvoices from "@/components/dashboard/PendingInvoices";
import RecurringExpenses from "@/components/dashboard/RecurringExpenses";
import { PageHeader } from "@/components/dashboard/DashboardLayout";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

export default function CashflowPage() {
  const [asOfUtc, setAsOfUtc] = useState<string>();
  const [openingBalance, setOpeningBalance] = useState(0);
  const [openingBalanceDateUtc, setOpeningBalanceDateUtc] = useState<string>();
  const [showSettings, setShowSettings] = useState(false);

  const handleToday = () => {
    const now = new Date();
    setAsOfUtc(now.toISOString());
  };

  const handleEndOfMonth = () => {
    const now = new Date();
    const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59, 999);
    setAsOfUtc(endOfMonth.toISOString());
  };

  const handleEndOfQuarter = () => {
    const now = new Date();
    const quarter = Math.floor(now.getMonth() / 3);
    const endOfQuarter = new Date(now.getFullYear(), (quarter + 1) * 3, 0, 23, 59, 59, 999);
    setAsOfUtc(endOfQuarter.toISOString());
  };

  return (
    <>
      <PageHeader
        title="التدفق النقدي"
        desc="متى يدخل المال ومتى يخرج — وخطّط لشهرك القادم بثقة."
      />

      {/* Quick Filters */}
      <div className="flex flex-wrap items-center gap-3 mb-6">
        <Button
          variant="outline"
          size="sm"
          onClick={handleToday}
          className="gap-2"
        >
          <CalendarDays className="w-4 h-4" />
          اليوم
        </Button>
        <Button
          variant="outline"
          size="sm"
          onClick={handleEndOfMonth}
          className="gap-2"
        >
          <CalendarDays className="w-4 h-4" />
          نهاية الشهر
        </Button>
        <Button
          variant="outline"
          size="sm"
          onClick={handleEndOfQuarter}
          className="gap-2"
        >
          <CalendarDays className="w-4 h-4" />
          نهاية الربع
        </Button>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => setShowSettings(!showSettings)}
          className="gap-2 ml-auto"
        >
          <Settings className="w-4 h-4" />
          إعدادات
        </Button>
      </div>

      {/* Settings Panel */}
      {showSettings && (
        <div className="bg-card rounded-xl p-4 border border-border/70 mb-6 space-y-4">
          <div className="grid md:grid-cols-3 gap-4">
            <div>
              <label className="text-sm font-medium text-navy mb-2 block">
                الرصيد الافتتاحي
              </label>
              <Input
                type="number"
                placeholder="0"
                value={openingBalance || ""}
                onChange={(e) => setOpeningBalance(Number(e.target.value) || 0)}
                className="text-sm"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-navy mb-2 block">
                تاريخ الرصيد الافتتاحي
              </label>
              <Input
                type="date"
                value={openingBalanceDateUtc ? new Date(openingBalanceDateUtc).toISOString().split('T')[0] : ""}
                onChange={(e) => {
                  if (e.target.value) {
                    setOpeningBalanceDateUtc(new Date(e.target.value).toISOString());
                  }
                }}
                className="text-sm"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-navy mb-2 block">
                حتى التاريخ
              </label>
              <Input
                type="date"
                value={asOfUtc ? new Date(asOfUtc).toISOString().split('T')[0] : ""}
                onChange={(e) => {
                  if (e.target.value) {
                    setAsOfUtc(new Date(e.target.value).toISOString());
                  }
                }}
                className="text-sm"
              />
            </div>
          </div>
        </div>
      )}

      {/* Summary Cards */}
      <div className="mb-8">
        <CashFlowSummary asOfUtc={asOfUtc} openingBalance={openingBalance} />
      </div>

      {/* Projections */}
      <div className="mb-8">
        <CashFlowProjection asOfUtc={asOfUtc} openingBalance={openingBalance} />
      </div>

      {/* Grid Layout */}
      <div className="grid md:grid-cols-2 gap-6 mb-8">
        <PendingInvoices asOfUtc={asOfUtc} openingBalance={openingBalance} />
        <RecurringExpenses asOfUtc={asOfUtc} openingBalance={openingBalance} />
      </div>
    </>
  );
}
