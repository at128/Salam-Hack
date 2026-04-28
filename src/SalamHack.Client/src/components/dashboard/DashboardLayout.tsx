import { useEffect, useState } from "react";
import { Bell, ChevronDown, KeyRound, LogOut, Search, UserRound } from "lucide-react";
import { Outlet, useNavigate } from "react-router-dom";
import Sidebar from "@/components/dashboard/Sidebar";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { clearAuthSession, fetchCurrentProfile, getCurrentUser, storeCurrentUser, type AuthUser } from "@/lib/auth";

type Props = {
  title?: string;
  subtitle?: string;
};

export default function DashboardLayout({
  title,
  subtitle = "إليك ملخص نشاطك المالي اليوم",
}: Props) {
  const navigate = useNavigate();
  const [user, setUser] = useState<AuthUser | null>(() => getCurrentUser());
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

  const handleLogout = () => {
    clearAuthSession();
    navigate("/login", { replace: true });
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
