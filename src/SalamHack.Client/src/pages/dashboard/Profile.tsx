import { useEffect, useState } from "react";
import { Loader2, Save, UserRound } from "lucide-react";
import { PageHeader } from "@/components/dashboard/DashboardLayout";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  fetchCurrentProfile,
  getCurrentUser,
  storeCurrentUser,
  updateCurrentProfile,
  type AuthUser,
} from "@/lib/auth";

type ProfileForm = {
  firstName: string;
  lastName: string;
  phoneNumber: string;
};

export default function ProfilePage() {
  const [profile, setProfile] = useState<AuthUser | null>(() => getCurrentUser());
  const [form, setForm] = useState<ProfileForm>({
    firstName: profile?.firstName ?? "",
    lastName: profile?.lastName ?? "",
    phoneNumber: profile?.phoneNumber ?? "",
  });
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;
    setIsLoading(true);

    fetchCurrentProfile()
      .then((result) => {
        if (!active) return;
        setProfile(result);
        storeCurrentUser(result);
        setForm({
          firstName: result.firstName ?? "",
          lastName: result.lastName ?? "",
          phoneNumber: result.phoneNumber ?? "",
        });
      })
      .catch(() => {
        if (!active) return;
        setError("تعذر تحميل بيانات الملف الشخصي.");
      })
      .finally(() => {
        if (!active) return;
        setIsLoading(false);
      });

    return () => {
      active = false;
    };
  }, []);

  const setField = (field: keyof ProfileForm) => (event: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [field]: event.target.value }));
    setMessage("");
    setError("");
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setIsSaving(true);
    setMessage("");
    setError("");

    try {
      const updated = await updateCurrentProfile({
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        phoneNumber: form.phoneNumber.trim(),
      });
      setProfile(updated);
      storeCurrentUser(updated);
      setMessage("تم تحديث بياناتك بنجاح.");
    } catch {
      setError("تعذر تحديث بياناتك. تحقق من المدخلات وحاول مرة أخرى.");
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <>
      <PageHeader title="الملف الشخصي" desc="إدارة اسمك ورقم هاتفك المستخدمين داخل لوحة التحكم." />

      <section className="max-w-2xl rounded-2xl border border-border/70 bg-card p-6 shadow-card">
        <div className="mb-5 flex items-center gap-3">
          <div className="grid h-12 w-12 place-items-center rounded-xl bg-teal-soft text-teal">
            <UserRound className="h-6 w-6" />
          </div>
          <div>
            <h3 className="font-bold text-navy">
              {profile ? `${profile.firstName} ${profile.lastName}`.trim() || profile.email : "بيانات الحساب"}
            </h3>
            <p className="text-xs text-muted-foreground">{profile?.email}</p>
          </div>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center rounded-xl bg-muted/40 p-8 text-sm text-muted-foreground">
            <Loader2 className="ml-2 h-4 w-4 animate-spin text-teal" />
            جاري تحميل بيانات الحساب...
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-4">
            {message && (
              <div className="rounded-xl border border-success/30 bg-success-soft p-3 text-sm text-success">
                {message}
              </div>
            )}
            {error && (
              <div className="rounded-xl border border-danger/30 bg-danger-soft p-3 text-sm text-danger">
                {error}
              </div>
            )}

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="firstName" className="text-navy">
                  الاسم الأول
                </Label>
                <Input
                  id="firstName"
                  value={form.firstName}
                  onChange={setField("firstName")}
                  required
                  className="h-11 rounded-xl border-border/70 bg-white"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="lastName" className="text-navy">
                  اسم العائلة
                </Label>
                <Input
                  id="lastName"
                  value={form.lastName}
                  onChange={setField("lastName")}
                  required
                  className="h-11 rounded-xl border-border/70 bg-white"
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="phoneNumber" className="text-navy">
                رقم الهاتف
              </Label>
              <Input
                id="phoneNumber"
                value={form.phoneNumber}
                onChange={setField("phoneNumber")}
                required
                className="h-11 rounded-xl border-border/70 bg-white"
              />
            </div>

            <Button
              type="submit"
              disabled={isSaving}
              className="rounded-xl bg-gradient-brand px-6 shadow-glow hover:opacity-90"
            >
              {isSaving ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : <Save className="ml-2 h-4 w-4" />}
              {isSaving ? "جاري الحفظ..." : "حفظ التغييرات"}
            </Button>
          </form>
        )}
      </section>
    </>
  );
}
