import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { ArrowLeft, Check, Eye, EyeOff, Loader2, Lock, Mail, Phone, User } from "lucide-react";
import AuthLayout from "@/components/auth/AuthLayout";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { storeAuthSession, type AuthSessionResponse } from "@/lib/auth";

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const REGISTER_API_URL = `${API_BASE_URL}/api/v1/Auth/register`;

const PERKS = [
  "حتى ٥ فواتير شهريا مجانا",
  "بدون بطاقة ائتمانية",
  "إلغاء في أي وقت",
];

type RegisterForm = {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  password: string;
};

type ValidationErrors = Partial<Record<keyof RegisterForm | "general", string[]>>;

function normalizeValidationErrors(payload: unknown): ValidationErrors {
  if (!payload || typeof payload !== "object") return {};

  const errors = (payload as { errors?: Record<string, string[]> }).errors;
  if (!errors || typeof errors !== "object") return {};

  const normalized: ValidationErrors = {};
  for (const [key, messages] of Object.entries(errors)) {
    if (!Array.isArray(messages)) continue;
    const field = key.charAt(0).toLowerCase() + key.slice(1);
    normalized[field as keyof RegisterForm] = messages;
  }

  return normalized;
}

function ErrorText({ messages }: { messages?: string[] }) {
  if (!messages?.length) return null;
  return <p className="text-xs leading-relaxed text-danger">{messages[0]}</p>;
}

export default function Register() {
  const navigate = useNavigate();
  const [showPwd, setShowPwd] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errors, setErrors] = useState<ValidationErrors>({});
  const [form, setForm] = useState<RegisterForm>({
    firstName: "",
    lastName: "",
    email: "",
    phoneNumber: "",
    password: "",
  });

  const setField = (field: keyof RegisterForm) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [field]: e.target.value }));
    setErrors((prev) => ({ ...prev, [field]: undefined, general: undefined }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setErrors({});

    try {
      const response = await fetch(REGISTER_API_URL, {
        method: "POST",
        credentials: "include",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          email: form.email.trim(),
          password: form.password,
          firstName: form.firstName.trim(),
          lastName: form.lastName.trim(),
          phoneNumber: form.phoneNumber.trim(),
        }),
      });

      const payload = await response.json().catch(() => null);

      if (!response.ok) {
        const validationErrors = normalizeValidationErrors(payload);
        setErrors(
          Object.keys(validationErrors).length > 0
            ? validationErrors
            : {
                general: [
                  (payload as { detail?: string; title?: string } | null)?.detail ??
                    "تعذر إنشاء الحساب. تحقق من البيانات وحاول مرة أخرى.",
                ],
              },
        );
        return;
      }

      storeAuthSession(payload as AuthSessionResponse);
      navigate("/dashboard");
    } catch {
      setErrors({ general: ["تعذر الاتصال بالخدمة. حاول مرة أخرى بعد قليل."] });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <AuthLayout
      badge="ابدأ مجانا"
      title="أنشئ حسابك في دقيقة"
      subtitle="انضم لمالي وابدأ بإدارة فواتيرك وأرباحك بكل وضوح."
      footer={
        <>
          لديك حساب بالفعل؟{" "}
          <Link to="/login" className="text-teal font-semibold hover:underline">
            سجل دخولك
          </Link>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-2.5">
        {errors.general && (
          <div className="rounded-xl border border-danger/30 bg-danger-soft p-3 text-xs leading-relaxed text-danger">
            {errors.general[0]}
          </div>
        )}

        <div className="grid gap-2.5 sm:grid-cols-2">
          <div className="space-y-1.5">
            <Label htmlFor="firstName" className="text-navy">
              الاسم الأول
            </Label>
            <div className="relative">
              <User className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                id="firstName"
                type="text"
                value={form.firstName}
                onChange={setField("firstName")}
                placeholder="محمد"
                required
                className="h-10 rounded-xl border-border/70 bg-card pr-10"
              />
            </div>
            <ErrorText messages={errors.firstName} />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="lastName" className="text-navy">
              اسم العائلة
            </Label>
            <div className="relative">
              <User className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                id="lastName"
                type="text"
                value={form.lastName}
                onChange={setField("lastName")}
                placeholder="العتيبي"
                required
                className="h-10 rounded-xl border-border/70 bg-card pr-10"
              />
            </div>
            <ErrorText messages={errors.lastName} />
          </div>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="email" className="text-navy">
            البريد الإلكتروني
          </Label>
          <div className="relative">
            <Mail className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              id="email"
              type="email"
              value={form.email}
              onChange={setField("email")}
              placeholder="name@example.com"
              required
              className="h-10 rounded-xl border-border/70 bg-card pr-10"
            />
          </div>
          <ErrorText messages={errors.email} />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="phoneNumber" className="text-navy">
            رقم الهاتف
          </Label>
          <div className="relative">
            <Phone className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              id="phoneNumber"
              type="tel"
              value={form.phoneNumber}
              onChange={setField("phoneNumber")}
              placeholder="+123456789"
              required
              className="h-10 rounded-xl border-border/70 bg-card pr-10"
            />
          </div>
          <ErrorText messages={errors.phoneNumber} />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="password" className="text-navy">
            كلمة المرور
          </Label>
          <div className="relative">
            <Lock className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              id="password"
              type={showPwd ? "text" : "password"}
              value={form.password}
              onChange={setField("password")}
              placeholder="8 أحرف على الأقل"
              required
              minLength={8}
              className="h-10 rounded-xl border-border/70 bg-card pr-10 pl-10"
            />
            <button
              type="button"
              onClick={() => setShowPwd((s) => !s)}
              className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-navy"
              aria-label="إظهار كلمة المرور"
            >
              {showPwd ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
            </button>
          </div>
          <ErrorText messages={errors.password} />
        </div>

        <label className="flex select-none items-start gap-2 text-xs leading-relaxed text-muted-foreground">
          <input type="checkbox" required className="mt-1 h-4 w-4 rounded border-border accent-teal" />
          <span>
            أوافق على{" "}
            <a href="#" className="text-teal hover:underline">
              شروط الاستخدام
            </a>{" "}
            و{" "}
            <a href="#" className="text-teal hover:underline">
              سياسة الخصوصية
            </a>
          </span>
        </label>

        <Button
          type="submit"
          size="lg"
          disabled={isSubmitting}
          className="h-10 w-full rounded-xl bg-gradient-brand text-sm shadow-glow hover:opacity-90"
        >
          {isSubmitting ? (
            <Loader2 className="ml-1 h-4 w-4 animate-spin" />
          ) : (
            <ArrowLeft className="ml-1 h-4 w-4" />
          )}
          {isSubmitting ? "جاري إنشاء الحساب..." : "إنشاء الحساب"}
        </Button>

        <ul className="grid gap-2 sm:grid-cols-3">
          {PERKS.map((perk) => (
            <li key={perk} className="flex items-center gap-1.5 text-xs text-muted-foreground">
              <span className="grid h-4 w-4 shrink-0 place-items-center rounded-full bg-success-soft text-success">
                <Check className="h-3 w-3" />
              </span>
              {perk}
            </li>
          ))}
        </ul>
      </form>
    </AuthLayout>
  );
}
