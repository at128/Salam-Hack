import { useEffect, useState } from "react";
import { Loader2, TrendingUp, AlertTriangle } from "lucide-react";
import { fetchCashFlowForecast } from "@/lib/reports";

interface Props {
  asOfUtc?: string;
  openingBalance?: number;
}

export default function CashFlowProjection({ asOfUtc, openingBalance = 0 }: Props) {
  const [projection, setProjection] = useState<any>(null);
  const [delayScenario, setDelayScenario] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    setLoading(true);
    setError("");

    fetchCashFlowForecast(asOfUtc, openingBalance)
      .then((data) => {
        setProjection(data.projection);
        setDelayScenario(data.clientDelayScenario);
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
        <h3 className="font-bold text-navy mb-1">التوقعات المستقبلية</h3>
        <p className="text-xs text-destructive">{error}</p>
      </div>
    );
  }

  if (!projection) {
    return null;
  }

  const toDate = new Date(projection.toUtc);
  const monthsAhead = Math.ceil((toDate.getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24 * 30));
  const expectedInflows = toAmount(projection.expectedInflows);
  const expectedOutflows = toAmount(projection.expectedOutflows);
  const expectedNetFlow = toAmount(projection.expectedNetFlow);
  const forecastBalance = toAmount(projection.forecastBalance);
  const projectionCards = [
    {
      label: "التدفقات الداخلة المتوقعة",
      value: expectedInflows,
      prefix: "+",
      valueClass: "text-green-600",
      cardClass: "bg-white/80 border-brand/10",
    },
    {
      label: "التدفقات الخارجة المتوقعة",
      value: expectedOutflows,
      prefix: "-",
      valueClass: "text-red-600",
      cardClass: "bg-white/80 border-brand/10",
    },
    {
      label: "صافي التدفق المتوقع",
      value: Math.abs(expectedNetFlow),
      prefix: expectedNetFlow >= 0 ? "+" : "-",
      valueClass: expectedNetFlow >= 0 ? "text-green-600" : "text-red-600",
      cardClass: expectedNetFlow >= 0 ? "bg-green-50 border-green-200" : "bg-red-50 border-red-200",
    },
    {
      label: "الرصيد المتوقع",
      value: forecastBalance,
      prefix: "",
      valueClass: forecastBalance >= 0 ? "text-brand" : "text-red-600",
      cardClass: "bg-brand/10 border-brand/20",
      hint: `بعد ~${monthsAhead} شهر`,
    },
  ];

  return (
    <div className="space-y-6">
      {/* Main Projection */}
      <div className="bg-gradient-to-br from-brand/10 to-brand/5 rounded-2xl p-6 border border-brand/20 shadow-card">
        <div className="flex items-center gap-2 mb-5">
          <TrendingUp className="w-5 h-5 text-brand" />
          <h3 className="font-bold text-navy">التوقع (الأشهر القادمة)</h3>
        </div>

        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          {projectionCards.map((card) => (
            <div key={card.label} className={`rounded-lg p-4 border ${card.cardClass}`}>
              <p className="text-xs text-muted-foreground mb-1">{card.label}</p>
              <p className={`text-2xl font-bold ${card.valueClass}`}>
                {card.prefix}{formatAmount(card.value)} ر.س
              </p>
              {card.hint && <p className="text-xs text-muted-foreground mt-2">{card.hint}</p>}
            </div>
          ))}
        </div>
      </div>

      {/* Client Delay Scenario */}
      {delayScenario && (
        <div className="bg-yellow-50 rounded-2xl p-6 border border-yellow-200 shadow-card">
          <div className="flex items-center gap-2 mb-5">
            <AlertTriangle className="w-5 h-5 text-yellow-600" />
            <h3 className="font-bold text-navy">سيناريو تأخير العملاء</h3>
          </div>

          <div className="space-y-4">
            <div className="bg-white/80 rounded-lg p-4 border border-yellow-100">
              <p className="text-xs text-muted-foreground mb-1">مدة التأخير</p>
              <p className="text-xl font-bold text-yellow-700">
                {delayScenario.customerName ?? "عميل محدد"}
              </p>
            </div>

            <div className="bg-white/80 rounded-lg p-4 border border-yellow-100">
              <p className="text-xs text-muted-foreground mb-1">المبلغ المتأثر</p>
              <p className="text-xl font-bold text-red-600">
                -{formatAmount(delayScenario.delayedAmount ?? delayScenario.affectedAmount)} ر.س
              </p>
            </div>

            <div className={`rounded-lg p-4 border transition-colors ${
              delayBalance(delayScenario) >= 0
                ? "bg-yellow-100 border-yellow-300"
                : "bg-red-50 border-red-200"
            }`}>
              <p className="text-xs text-muted-foreground mb-1">الرصيد المتوقع مع التأخير</p>
              <p className={`text-xl font-bold ${
                delayBalance(delayScenario) >= 0
                  ? "text-yellow-700"
                  : "text-red-600"
              }`}>
                {formatAmount(delayBalance(delayScenario))} ر.س
              </p>
              <p className="text-xs mt-2">
                {delayBalance(delayScenario) < 0 ? (
                  <span className="text-red-600">⚠️ قد تواجه عجز في الرصيد</span>
                ) : (
                  <span className="text-yellow-700">⚠️ تأثير على التدفق النقدي</span>
                )}
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function toAmount(value: unknown) {
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : 0;
}

function formatAmount(value: unknown) {
  return Math.round(toAmount(value)).toLocaleString();
}

function delayBalance(delayScenario: any) {
  return toAmount(delayScenario.forecastBalanceAfterDelay ?? delayScenario.projectedBalanceWithDelay);
}
