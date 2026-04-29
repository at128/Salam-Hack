import { useEffect, useState } from "react";
import { Bell, ChevronDown, KeyRound, Loader2, LogOut, Plus, Search, Trash2, UserRound } from "lucide-react";
import { Outlet, useNavigate } from "react-router-dom";
import Sidebar from "@/components/dashboard/Sidebar";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import {
  clearAuthSession,
  fetchCurrentProfile,
  getApiErrorMessage,
  getCurrentUser,
  getValidAccessToken,
  storeCurrentUser,
  unwrapApiResponse,
  type AuthUser,
} from "@/lib/auth";

type Props = {
  title?: string;
  subtitle?: string;
};

type ServiceCategory = "Design" | "Development" | "Consulting" | "Marketing" | "Content" | "Other";

type ServiceOnboardingRow = {
  id: string;
  serviceName: string;
  category: ServiceCategory;
  defaultHourlyRate: string;
  defaultRevisions: string;
  isActive: boolean;
};

type PaginatedList<T> = {
  totalCount: number;
  items: T[];
};

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const SERVICES_API_URL = `${API_BASE_URL}/api/v1/services`;

const CATEGORY_OPTIONS: { value: ServiceCategory; label: string }[] = [
  { value: "Design", label: "تصميم" },
  { value: "Development", label: "تطوير" },
  { value: "Consulting", label: "استشارات" },
  { value: "Marketing", label: "تسويق" },
  { value: "Content", label: "محتوى" },
  { value: "Other", label: "أخرى" },
];

function createEmptyServiceRow(): ServiceOnboardingRow {
  return {
    id: crypto.randomUUID(),
    serviceName: "",
    category: "Design",
    defaultHourlyRate: "",
    defaultRevisions: "0",
    isActive: true,
  };
}

async function servicesRequest<T>(path = "", init?: RequestInit): Promise<T> {
  const token = await getValidAccessToken();
  if (!token) throw new Error("Missing access token.");

  const response = await fetch(`${SERVICES_API_URL}${path}`, {
    ...init,
    headers: {
      Accept: "application/json",
      Authorization: `Bearer ${token}`,
      ...(init?.body ? { "Content-Type": "application/json" } : {}),
      ...init?.headers,
    },
  });

  const payload = await response.json().catch(() => null);

  if (!response.ok) {
    throw payload ?? new Error(getApiErrorMessage(payload, "تعذر تنفيذ الطلب."));
  }

  return unwrapApiResponse<T>(payload);
}

function RequiredMark() {
  return <span className="text-danger">*</span>;
}

export default function DashboardLayout({
  title,
  subtitle = "إليك ملخص نشاطك المالي اليوم",
}: Props) {
  const navigate = useNavigate();
  const [user, setUser] = useState<AuthUser | null>(() => getCurrentUser());
  const [showServicesOnboarding, setShowServicesOnboarding] = useState(false);
  const [hasCheckedServices, setHasCheckedServices] = useState(false);
  const [serviceRows, setServiceRows] = useState<ServiceOnboardingRow[]>(() => [createEmptyServiceRow()]);
  const [isSavingServices, setIsSavingServices] = useState(false);
  const [servicesError, setServicesError] = useState("");
  const displayName = user ? `${user.firstName} ${user.lastName}`.trim() || user.email : "المستخدم";
  const headerTitle = title ?? `مرحبا، ${displayName}`;

  useEffect(() => {
    let active = true;

    const syncUser = () => setUser(getCurrentUser());
    window.addEventListener("auth:user-updated", syncUser);

    fetchCurrentProfile()
      .then((profile) => {
        if (!active) return;
        storeCurrentUser(profile);
        setUser(profile);
      })
      .catch(() => {
        if (!active) return;
        syncUser();
      });

    return () => {
      active = false;
      window.removeEventListener("auth:user-updated", syncUser);
    };
  }, []);

  useEffect(() => {
    let active = true;

    async function checkServices() {
      try {
        const result = await servicesRequest<PaginatedList<unknown>>("?includeInactive=true&pageSize=1");
        if (!active) return;
        setHasCheckedServices(true);
        setShowServicesOnboarding(result.totalCount === 0);
      } catch {
        if (!active) return;
        setHasCheckedServices(true);
      }
    }

    void checkServices();

    return () => {
      active = false;
    };
  }, []);

  const handleLogout = () => {
    clearAuthSession();
    navigate("/login", { replace: true });
  };

  const setServiceField = <K extends keyof ServiceOnboardingRow>(
    rowId: string,
    field: K,
    value: ServiceOnboardingRow[K],
  ) => {
    setServiceRows((rows) => rows.map((row) => (row.id === rowId ? { ...row, [field]: value } : row)));
    setServicesError("");
  };

  const addServiceRow = () => {
    setServiceRows((rows) => [...rows, createEmptyServiceRow()]);
  };

  const removeServiceRow = (rowId: string) => {
    setServiceRows((rows) => (rows.length > 1 ? rows.filter((row) => row.id !== rowId) : rows));
  };

  const saveServices = async (event: React.FormEvent) => {
    event.preventDefault();
    setServicesError("");

    const validRows = serviceRows.map((row) => ({
      ...row,
      serviceName: row.serviceName.trim(),
      defaultHourlyRate: Number(row.defaultHourlyRate),
      defaultRevisions: Number(row.defaultRevisions),
    }));

    if (validRows.some((row) => !row.serviceName || row.defaultHourlyRate <= 0 || row.defaultRevisions < 0)) {
      setServicesError("أدخل اسم الخدمة وسعر الساعة والتعديلات بشكل صحيح.");
      return;
    }

    setIsSavingServices(true);

    try {
      for (const row of validRows) {
        await servicesRequest("", {
          method: "POST",
          body: JSON.stringify({
            serviceName: row.serviceName,
            category: row.category,
            defaultHourlyRate: row.defaultHourlyRate,
            defaultRevisions: row.defaultRevisions,
            isActive: row.isActive,
          }),
        });
      }

      setShowServicesOnboarding(false);
    } catch (error) {
      setServicesError(getApiErrorMessage(error, "تعذر حفظ الخدمات. تحقق من البيانات وحاول مرة أخرى."));
    } finally {
      setIsSavingServices(false);
    }
  };

  return (
    <div className="min-h-screen bg-background">
      <Sidebar user={user} onLogout={handleLogout} />

      <div className="lg:pr-60">
        <header className="sticky top-0 z-30 border-b border-border/60 bg-background/85 backdrop-blur">
          <div className="grid h-16 grid-cols-3 items-center px-6">
            <div>
              <h1 className="text-lg font-bold text-navy">{headerTitle}</h1>
              <p className="text-xs text-muted-foreground">{subtitle}</p>
            </div>

            <div className="flex justify-center">
              <div className="hidden items-center gap-2 rounded-full bg-muted/60 px-3 py-1.5 text-sm md:flex">
                <Search className="h-4 w-4 text-muted-foreground" />
                <input
                  className="w-44 bg-transparent text-sm outline-none placeholder:text-muted-foreground"
                  placeholder="ابحث عن فاتورة، عميل..."
                />
              </div>
            </div>

            <div className="flex justify-end gap-2">
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <button
                    type="button"
                    className="hidden items-center gap-2 rounded-full border border-border/70 bg-card px-3 py-1.5 transition-colors hover:bg-muted/40 sm:flex"
                    aria-label="قائمة الحساب"
                  >
                    <div className="grid h-7 w-7 place-items-center rounded-full bg-gradient-brand text-xs font-bold text-white">
                      {displayName.charAt(0)}
                    </div>
                    <span className="max-w-32 truncate text-sm font-semibold text-navy">{displayName}</span>
                    <ChevronDown className="h-3.5 w-3.5 text-muted-foreground" />
                  </button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-56 text-right" dir="rtl">
                  <DropdownMenuLabel>
                    <div className="min-w-0">
                      <p className="truncate text-sm font-semibold text-navy">{displayName}</p>
                      <p className="truncate text-xs font-normal text-muted-foreground">{user?.email}</p>
                    </div>
                  </DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={() => navigate("/dashboard/profile")} className="cursor-pointer justify-start gap-2 text-right">
                    <UserRound className="h-4 w-4" />
                    الملف الشخصي
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={() => navigate("/dashboard/change-password")} className="cursor-pointer justify-start gap-2 text-right">
                    <KeyRound className="h-4 w-4" />
                    تغيير كلمة المرور
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={handleLogout} className="cursor-pointer justify-start gap-2 text-right text-danger focus:text-danger">
                    <LogOut className="h-4 w-4" />
                    تسجيل الخروج
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
              <Button size="icon" variant="outline" className="rounded-full">
                <Bell className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </header>

        <main className="space-y-6 p-6">
          <Outlet />
        </main>
      </div>

      {hasCheckedServices && (
        <Dialog open={showServicesOnboarding} onOpenChange={setShowServicesOnboarding}>
          <DialogContent className="max-h-[90vh] max-w-3xl overflow-y-auto text-right" dir="rtl">
            <DialogHeader className="text-right sm:text-right">
              <DialogTitle>ما الخدمات التي تقدمها؟</DialogTitle>
              <DialogDescription>
                أضف خدماتك الأساسية الآن حتى نستخدمها في التسعير، المشاريع، والتحليلات.
              </DialogDescription>
            </DialogHeader>

            <form onSubmit={saveServices} className="space-y-4">
              {servicesError && (
                <div className="rounded-xl border border-danger/30 bg-danger-soft p-3 text-sm text-danger">
                  {servicesError}
                </div>
              )}

              <div className="space-y-3">
                {serviceRows.map((row, index) => (
                  <div key={row.id} className="rounded-2xl border border-border/70 bg-card p-4">
                    <div className="mb-3 flex items-center justify-between">
                      <h3 className="font-bold text-navy">خدمة {index + 1}</h3>
                      <Button
                        type="button"
                        variant="outline"
                        size="icon"
                        className="h-8 w-8 rounded-xl text-danger hover:text-danger"
                        disabled={serviceRows.length === 1}
                        onClick={() => removeServiceRow(row.id)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>

                    <div className="grid gap-3 md:grid-cols-2">
                      <div className="space-y-2">
                        <Label htmlFor={`serviceName-${row.id}`} className="flex items-center gap-1 text-navy">
                          اسم الخدمة <RequiredMark />
                        </Label>
                        <Input
                          id={`serviceName-${row.id}`}
                          value={row.serviceName}
                          onChange={(event) => setServiceField(row.id, "serviceName", event.target.value)}
                          placeholder="مثال: تصميم واجهات"
                          required
                          className="rounded-xl bg-white"
                        />
                      </div>

                      <div className="space-y-2">
                        <Label className="flex items-center gap-1 text-navy">
                          التصنيف <RequiredMark />
                        </Label>
                        <Select
                          dir="rtl"
                          value={row.category}
                          onValueChange={(value) => setServiceField(row.id, "category", value as ServiceCategory)}
                        >
                          <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                            <SelectValue />
                          </SelectTrigger>
                          <SelectContent dir="rtl" className="text-right">
                            {CATEGORY_OPTIONS.map((option) => (
                              <SelectItem key={option.value} value={option.value}>
                                {option.label}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>

                      <div className="space-y-2">
                        <Label htmlFor={`rate-${row.id}`} className="flex items-center gap-1 text-navy">
                          سعر الساعة الافتراضي <RequiredMark />
                        </Label>
                        <Input
                          id={`rate-${row.id}`}
                          type="number"
                          min="1"
                          step="0.01"
                          value={row.defaultHourlyRate}
                          onChange={(event) => setServiceField(row.id, "defaultHourlyRate", event.target.value)}
                          required
                          className="rounded-xl bg-white"
                        />
                      </div>

                      <div className="space-y-2">
                        <Label htmlFor={`revisions-${row.id}`} className="flex items-center gap-1 text-navy">
                          عدد التعديلات الافتراضي <RequiredMark />
                        </Label>
                        <Input
                          id={`revisions-${row.id}`}
                          type="number"
                          min="0"
                          step="1"
                          value={row.defaultRevisions}
                          onChange={(event) => setServiceField(row.id, "defaultRevisions", event.target.value)}
                          required
                          className="rounded-xl bg-white"
                        />
                      </div>
                    </div>

                    <label className="mt-3 flex items-center gap-2 text-sm text-muted-foreground">
                      <Switch
                        checked={row.isActive}
                        onCheckedChange={(checked) => setServiceField(row.id, "isActive", checked)}
                      />
                      خدمة نشطة
                    </label>
                  </div>
                ))}
              </div>

              <Button type="button" variant="outline" className="rounded-xl" onClick={addServiceRow}>
                <Plus className="ml-2 h-4 w-4" />
                إضافة خدمة أخرى
              </Button>

              <DialogFooter className="gap-2 sm:justify-start sm:space-x-0">
                <Button type="submit" disabled={isSavingServices} className="rounded-xl bg-gradient-brand shadow-glow hover:opacity-90">
                  {isSavingServices ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : null}
                  حفظ الخدمات
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  className="rounded-xl"
                  onClick={() => setShowServicesOnboarding(false)}
                >
                  لاحقاً
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      )}
    </div>
  );
}

export function PageHeader({ title, desc }: { title: string; desc?: string }) {
  return (
    <div className="mb-2">
      <h2 className="text-2xl font-bold text-navy">{title}</h2>
      {desc && <p className="mt-1 text-sm text-muted-foreground">{desc}</p>}
    </div>
  );
}
