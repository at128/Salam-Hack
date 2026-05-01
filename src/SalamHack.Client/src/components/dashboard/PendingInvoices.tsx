import { useEffect, useState } from "react";
import { Loader2, AlertCircle, Clock } from "lucide-react";
import { fetchCashFlowForecast } from "@/lib/reports";

interface PendingInvoice {
  invoiceId: string;
  customerName: string;
  amount: number;
  dueUtc: string;
  daysPastDue: number;
}

interface Props {
  asOfUtc?: string;
  openingBalance?: number;
}

export default function PendingInvoices({ asOfUtc, openingBalance = 0 }: Props) {
  const [invoices, setInvoices] = useState<PendingInvoice[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    setLoading(true);
    setError("");

    fetchCashFlowForecast(asOfUtc, openingBalance)
      .then((data) => {
        setInvoices(data.pendingInvoices || []);
      })
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  }, [asOfUtc, openingBalance]);

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
        <h3 className="font-bold text-navy mb-1">الفواتير قيد التحصيل</h3>
        <p className="text-xs text-destructive">{error}</p>
      </div>
    );
  }

  if (invoices.length === 0) {
    return (
      <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
        <h3 className="font-bold text-navy mb-1">الفواتير قيد التحصيل</h3>
        <p className="text-xs text-muted-foreground">جميع فواتيرك مدفوعة! 🎉</p>
      </div>
    );
  }

  const totalPending = invoices.reduce((sum, inv) => sum + inv.amount, 0);
  const overdueInvoices = invoices.filter((inv) => inv.daysPastDue > 0);

  return (
    <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
      <div className="flex items-center justify-between mb-5">
        <div>
          <h3 className="font-bold text-navy">الفواتير قيد التحصيل</h3>
          <p className="text-xs text-muted-foreground mt-1">
            {invoices.length} فاتورة بقيمة {Math.round(totalPending).toLocaleString()} ر.س
          </p>
        </div>
        {overdueInvoices.length > 0 && (
          <div className="flex items-center gap-2 px-3 py-1 bg-red-50 rounded-lg border border-red-200">
            <AlertCircle className="w-4 h-4 text-red-600" />
            <span className="text-xs font-medium text-red-700">{overdueInvoices.length} متأخرة</span>
          </div>
        )}
      </div>

      <div className="space-y-3">
        {invoices.map((invoice) => {
          const dueDate = new Date(invoice.dueUtc);
          const isOverdue = invoice.daysPastDue > 0;
          
          return (
            <div
              key={invoice.invoiceId}
              className={`p-3 rounded-lg border transition-colors ${
                isOverdue
                  ? "bg-red-50 border-red-200"
                  : "bg-muted/30 border-border/50"
              }`}
            >
              <div className="flex items-start justify-between mb-1">
                <div className="flex-1">
                  <p className="text-sm font-medium text-navy">{invoice.customerName}</p>
                  <p className="text-xs text-muted-foreground mt-0.5">
                    فاتورة رقم: {invoice.invoiceId}
                  </p>
                </div>
                <span className={`font-bold text-sm ${
                  isOverdue ? "text-red-600" : "text-navy"
                }`}>
                  {Math.round(invoice.amount).toLocaleString()} ر.س
                </span>
              </div>
              <div className="flex items-center gap-2 text-xs">
                <Clock className={`w-3.5 h-3.5 ${
                  isOverdue ? "text-red-600" : "text-muted-foreground"
                }`} />
                <span className={isOverdue ? "text-red-600 font-medium" : "text-muted-foreground"}>
                  {isOverdue
                    ? `متأخرة ${invoice.daysPastDue} أيام`
                    : `استحقاق: ${dueDate.toLocaleDateString("ar-SA")}`}
                </span>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
