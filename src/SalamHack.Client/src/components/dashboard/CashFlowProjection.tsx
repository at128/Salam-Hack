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

  const fromDate = new Date(projection.fromUtc);
  const toDate = new Date(projection.toUtc);
  const monthsAhead = Math.ceil((toDate.getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24 * 30));

  return (
    <div className="grid md:grid-cols-2 gap-6">
      {/* Main Projection */}
      <div className="bg-gradient-to-br from-brand/10 to-brand/5 rounded-2xl p-6 border border-brand/20 shadow-card">
        <div className="flex items-center gap-2 mb-5">
          <TrendingUp className="w-5 h-5 text-brand" />
          <h3 className="font-bold text-navy">التوقع (الأشهر القادمة)</h3>
        </div>

        <div className="space-y-4">
          <div className="bg-white/80 rounded-lg p-4 border border-brand/10">
            <p className="text-xs text-muted-foreground mb-1">التدفقات الداخلة المتوقعة</p>
            <p className="text-2xl font-bold text-green-600">
              +{Math.round(projection.expectedInflows).toLocaleString()} ر.س
            </p>
          </div>

          <div className="bg-white/80 rounded-lg p-4 border border-brand/10">
            <p className="text-xs text-muted-foreground mb-1">التدفقات الخارجة المتوقعة</p>
            <p className="text-2xl font-bold text-red-600">
              -{Math.round(projection.expectedOutflows).toLocaleString()} ر.س
            </p>
          </div>

          <div className={`rounded-lg p-4 border ${
            projection.expectedNetFlow >= 0
              ? "bg-green-50 border-green-200"
              : "bg-red-50 border-red-200"
          }`}>
            <p className="text-xs text-muted-foreground mb-1">صافي التدفق المتوقع</p>
            <p className={`text-2xl font-bold ${
              projection.expectedNetFlow >= 0 ? "text-green-600" : "text-red-600"
            }`}>
              {projection.expectedNetFlow >= 0 ? "+" : "-"}
              {Math.round(Math.abs(projection.expectedNetFlow)).toLocaleString()} ر.س
            </p>
          </div>

          <div className="bg-brand/10 rounded-lg p-4 border border-brand/20">
            <p className="text-xs text-muted-foreground mb-1">الرصيد المتوقع</p>
            <p className={`text-2xl font-bold ${
              projection.forecastBalance >= 0 ? "text-brand" : "text-red-600"
            }`}>
              {Math.round(projection.forecastBalance).toLocaleString()} ر.س
            </p>
            <p className="text-xs text-muted-foreground mt-2">
              بعد ~{monthsAhead} شهر
            </p>
          </div>
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
                {delayScenario.delayDays} أيام
              </p>
            </div>

            <div className="bg-white/80 rounded-lg p-4 border border-yellow-100">
              <p className="text-xs text-muted-foreground mb-1">المبلغ المتأثر</p>
              <p className="text-xl font-bold text-red-600">
                -{Math.round(delayScenario.affectedAmount).toLocaleString()} ر.س
              </p>
            </div>

            <div className={`rounded-lg p-4 border transition-colors ${
              delayScenario.projectedBalanceWithDelay >= 0
                ? "bg-yellow-100 border-yellow-300"
                : "bg-red-50 border-red-200"
            }`}>
              <p className="text-xs text-muted-foreground mb-1">الرصيد المتوقع مع التأخير</p>
              <p className={`text-xl font-bold ${
                delayScenario.projectedBalanceWithDelay >= 0
                  ? "text-yellow-700"
                  : "text-red-600"
              }`}>
                {Math.round(delayScenario.projectedBalanceWithDelay).toLocaleString()} ر.س
              </p>
              <p className="text-xs mt-2">
                {delayScenario.projectedBalanceWithDelay < 0 ? (
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
