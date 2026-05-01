import { useEffect, useState } from "react";
import { Loader2 } from "lucide-react";
import { fetchProfitabilityReport, ProfitabilityReportDto } from "@/lib/reports";

interface CustomerProfitBreakdown {
  entityId: string;
  name: string;
  revenue: number;
  cost: number;
  profit: number;
  marginPercent: number;
}

interface Props {
  fromUtc?: string;
  toUtc?: string;
}

export default function CustomerProfits({ fromUtc, toUtc }: Props) {
  const [data, setData] = useState<CustomerProfitBreakdown[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    setLoading(true);
    setError("");
    
    fetchProfitabilityReport(fromUtc, toUtc)
      .then((report) => {
        const customers = (report.byCustomer || []) as CustomerProfitBreakdown[];
        setData(customers.sort((a, b) => b.profit - a.profit));
      })
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  }, [fromUtc, toUtc]);

  if (loading) {
    return (
      <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card flex items-center justify-center h-64">
        <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
        <h3 className="font-bold text-navy mb-1">الأرباح حسب العميل</h3>
        <p className="text-xs text-destructive">{error}</p>
      </div>
    );
  }

  if (data.length === 0) {
    return (
      <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
        <h3 className="font-bold text-navy mb-1">الأرباح حسب العميل</h3>
        <p className="text-xs text-muted-foreground">لا توجد بيانات متاحة للفترة المختارة</p>
      </div>
    );
  }

  const max = Math.max(...data.map((c) => c.profit));
  
  return (
    <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
      <h3 className="font-bold text-navy mb-1">الأرباح حسب العميل</h3>
      <p className="text-xs text-muted-foreground mb-5">
        أي عملاء يجلبون أعلى ربحية؟
      </p>
      <div className="space-y-4">
        {data.map((c) => (
          <div key={c.entityId}>
            <div className="flex items-center justify-between text-sm mb-1.5">
              <span className="text-navy font-medium">{c.name}</span>
              <div className="flex items-center gap-2">
                <span className="text-xs text-muted-foreground">{c.marginPercent.toFixed(1)}٪</span>
                <span className="font-bold text-navy">{Math.round(c.profit).toLocaleString()} ر.س</span>
              </div>
            </div>
            <div className="h-2 rounded-full bg-muted overflow-hidden">
              <div
                className="h-full bg-gradient-brand rounded-full transition-all duration-300"
                style={{ width: `${max > 0 ? (c.profit / max) * 100 : 0}%` }}
              />
            </div>
            <div className="flex justify-between text-xs text-muted-foreground mt-1">
              <span>إيراد: {Math.round(c.revenue).toLocaleString()} ر.س</span>
              <span>مصاريف: {Math.round(c.cost).toLocaleString()} ر.س</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
