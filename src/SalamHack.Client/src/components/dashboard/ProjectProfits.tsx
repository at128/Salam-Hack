import { useEffect, useState } from "react";
import { Loader2, Trophy, TrendingDown } from "lucide-react";
import { fetchProfitabilityReport } from "@/lib/reports";

interface ProjectProfitBreakdown {
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

export default function ProjectProfits({ fromUtc, toUtc }: Props) {
  const [topPerformers, setTopPerformers] = useState<ProjectProfitBreakdown[]>([]);
  const [lowestPerformers, setLowestPerformers] = useState<ProjectProfitBreakdown[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    setLoading(true);
    setError("");
    
    fetchProfitabilityReport(fromUtc, toUtc)
      .then((report) => {
        const top = (report.topPerformers || []) as ProjectProfitBreakdown[];
        const lowest = (report.lowestPerformers || []) as ProjectProfitBreakdown[];
        setTopPerformers(top);
        setLowestPerformers(lowest);
      })
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  }, [fromUtc, toUtc]);

  if (loading) {
    return (
      <div className="grid md:grid-cols-2 gap-6">
        {[1, 2].map((i) => (
          <div key={i} className="bg-card rounded-2xl p-6 border border-border/70 shadow-card flex items-center justify-center h-64">
            <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
          </div>
        ))}
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
        <p className="text-xs text-destructive">{error}</p>
      </div>
    );
  }

  return (
    <div className="grid md:grid-cols-2 gap-6">
      {/* Top Performers */}
      <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
        <div className="flex items-center gap-2 mb-5">
          <Trophy className="w-5 h-5 text-amber-500" />
          <h3 className="font-bold text-navy">أفضل المشاريع ربحية</h3>
        </div>
        {topPerformers.length === 0 ? (
          <p className="text-xs text-muted-foreground">لا توجد بيانات متاحة</p>
        ) : (
          <div className="space-y-3">
            {topPerformers.map((p, i) => (
              <div key={p.entityId} className="pb-3 border-b border-border/50 last:border-b-0 last:pb-0">
                <div className="flex items-start justify-between mb-1">
                  <div>
                    <span className="text-sm font-medium text-navy">#{i + 1} {p.name}</span>
                    <p className="text-xs text-muted-foreground mt-0.5">
                      هامش: {p.marginPercent.toFixed(1)}%
                    </p>
                  </div>
                  <span className="font-bold text-green-600 text-sm">{Math.round(p.profit).toLocaleString()} ر.س</span>
                </div>
                <div className="text-xs text-muted-foreground space-y-0.5">
                  <div>الإيراد: {Math.round(p.revenue).toLocaleString()} ر.س</div>
                  <div>المصاريف: {Math.round(p.cost).toLocaleString()} ر.س</div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Lowest Performers */}
      <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
        <div className="flex items-center gap-2 mb-5">
          <TrendingDown className="w-5 h-5 text-red-500" />
          <h3 className="font-bold text-navy">المشاريع بأقل ربحية</h3>
        </div>
        {lowestPerformers.length === 0 ? (
          <p className="text-xs text-muted-foreground">لا توجد بيانات متاحة</p>
        ) : (
          <div className="space-y-3">
            {lowestPerformers.map((p, i) => (
              <div key={p.entityId} className="pb-3 border-b border-border/50 last:border-b-0 last:pb-0">
                <div className="flex items-start justify-between mb-1">
                  <div>
                    <span className="text-sm font-medium text-navy">#{i + 1} {p.name}</span>
                    <p className="text-xs text-muted-foreground mt-0.5">
                      هامش: {p.marginPercent.toFixed(1)}%
                    </p>
                  </div>
                  <span className={`font-bold text-sm ${p.profit >= 0 ? 'text-orange-600' : 'text-red-600'}`}>
                    {Math.round(p.profit).toLocaleString()} ر.س
                  </span>
                </div>
                <div className="text-xs text-muted-foreground space-y-0.5">
                  <div>الإيراد: {Math.round(p.revenue).toLocaleString()} ر.س</div>
                  <div>المصاريف: {Math.round(p.cost).toLocaleString()} ر.س</div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
