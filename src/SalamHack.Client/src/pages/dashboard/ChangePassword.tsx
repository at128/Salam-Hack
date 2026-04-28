import { useState } from "react";
import { Eye, EyeOff, KeyRound, Loader2, Save } from "lucide-react";
import { PageHeader } from "@/components/dashboard/DashboardLayout";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { changeCurrentPassword } from "@/lib/auth";

type PasswordForm = {
  currentPassword: string;
  newPassword: string;
};

type ValidationErrors = Partial<Record<keyof PasswordForm | "general", string[]>>;

function normalizeValidationErrors(payload: unknown): ValidationErrors {
  if (!payload || typeof payload !== "object") return {};

  const errors = (payload as { errors?: Record<string, string[]> }).errors;
  if (!errors || typeof errors !== "object") return {};

  const normalized: ValidationErrors = {};
  for (const [key, messages] of Object.entries(errors)) {
    if (!Array.isArray(messages)) continue;
    const field = key.charAt(0).toLowerCase() + key.slice(1);
    normalized[field as keyof PasswordForm] = messages;
  }

  return normalized;
}

function ErrorList({ messages }: { messages?: string[] }) {
  if (!messages?.length) return null;

  return (
    <ul className="space-y-1 text-xs leading-relaxed text-danger">
      {messages.map((message) => (
        <li key={message}>{message}</li>
      ))}
    </ul>
  );
}

export default function ChangePasswordPage() {
  const [form, setForm] = useState<PasswordForm>({
    currentPassword: "",
    newPassword: "",
  });
  const [showCurrentPassword, setShowCurrentPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState("");
  const [errors, setErrors] = useState<ValidationErrors>({});

  const setField = (field: keyof PasswordForm) => (event: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [field]: event.target.value }));
    setErrors((prev) => ({ ...prev, [field]: undefined, general: undefined }));
    setMessage("");
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setIsSaving(true);
    setMessage("");
    setErrors({});

    try {
      await changeCurrentPassword({
        currentPassword: form.currentPassword,
        newPassword: form.newPassword,
      });
      setForm({ currentPassword: "", newPassword: "" });
      setMessage("تم تغيير كلمة المرور بنجاح.");
    } catch (error) {
      const validationErrors = normalizeValidationErrors(error);
      setErrors(
        Object.keys(validationErrors).length > 0
          ? validationErrors
          : {
              general: [
                (error as { detail?: string; title?: string } | null)?.detail ??
                  "تعذر تغيير كلمة المرور. تحقق من البيانات وحاول مرة أخرى.",
              ],
            },
      );
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <>
      <PageHeader title="تغيير كلمة المرور" desc="حدّث كلمة مرور حسابك للحفاظ على أمان بياناتك." />

      <section className="max-w-2xl rounded-2xl border border-border/70 bg-card p-6 shadow-card">
        <div className="mb-5 flex items-center gap-3">
          <div className="grid h-12 w-12 place-items-center rounded-xl bg-teal-soft text-teal">
            <KeyRound className="h-6 w-6" />
          </div>
          <div>
            <h3 className="font-bold text-navy">أمان الحساب</h3>
            <p className="text-xs text-muted-foreground">استخدم كلمة مرور قوية ومختلفة عن السابقة.</p>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          {message && (
            <div className="rounded-xl border border-success/30 bg-success-soft p-3 text-sm text-success">
              {message}
            </div>
          )}
          {errors.general && (
            <div className="rounded-xl border border-danger/30 bg-danger-soft p-3 text-sm text-danger">
              {errors.general[0]}
            </div>
          )}

          <div className="space-y-2">
            <Label htmlFor="currentPassword" className="text-navy">
              كلمة المرور الحالية
            </Label>
            <div className="relative">
              <Input
                id="currentPassword"
                type={showCurrentPassword ? "text" : "password"}
                value={form.currentPassword}
                onChange={setField("currentPassword")}
                required
                className="h-11 rounded-xl border-border/70 bg-white pl-10"
              />
              <button
                type="button"
                onClick={() => setShowCurrentPassword((value) => !value)}
                className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-navy"
                aria-label="إظهار كلمة المرور الحالية"
              >
                {showCurrentPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
              </button>
            </div>
            <ErrorList messages={errors.currentPassword} />
          </div>

          <div className="space-y-2">
            <Label htmlFor="newPassword" className="text-navy">
              كلمة المرور الجديدة
            </Label>
            <div className="relative">
              <Input
                id="newPassword"
                type={showNewPassword ? "text" : "password"}
                value={form.newPassword}
                onChange={setField("newPassword")}
                required
                minLength={8}
                className="h-11 rounded-xl border-border/70 bg-white pl-10"
              />
              <button
                type="button"
                onClick={() => setShowNewPassword((value) => !value)}
                className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-navy"
                aria-label="إظهار كلمة المرور الجديدة"
              >
                {showNewPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
              </button>
            </div>
            <ErrorList messages={errors.newPassword} />
          </div>

          <Button
            type="submit"
            disabled={isSaving}
            className="rounded-xl bg-gradient-brand px-6 shadow-glow hover:opacity-90"
          >
            {isSaving ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : <Save className="ml-2 h-4 w-4" />}
            {isSaving ? "جاري الحفظ..." : "تغيير كلمة المرور"}
          </Button>
        </form>
      </section>
    </>
  );
}
