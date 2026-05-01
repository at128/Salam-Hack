import { useState } from "react";
import { Link, Navigate, useLocation, useNavigate } from "react-router-dom";
import { ArrowLeft, Loader2, MailCheck } from "lucide-react";
import AuthLayout from "@/components/auth/AuthLayout";
import { Button } from "@/components/ui/button";
import { InputOTP, InputOTPGroup, InputOTPSlot } from "@/components/ui/input-otp";
import { storeAuthSession, unwrapApiResponse, type AuthSessionResponse } from "@/lib/auth";
import type { RegisterForm } from "./Register";

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const VERIFY_REGISTER_API_URL = `${API_BASE_URL}/api/v1/Auth/register/verify`;

type LocationState = {
  registrationData?: RegisterForm;
};

function getApiErrorMessage(payload: unknown) {
  if (!payload || typeof payload !== "object") {
    return "تعذر التحقق من الرمز. حاول مرة أخرى.";
  }

  const response = payload as {
    message?: string;
    detail?: string;
    title?: string;
    errors?: Array<{ message?: string }>;
  };

  return response.errors?.[0]?.message ?? response.message ?? response.detail ?? response.title ?? "تعذر التحقق من الرمز. حاول مرة أخرى.";
}

export default function RegisterVerify() {
  const navigate = useNavigate();
  const location = useLocation();
  const registrationData = (location.state as LocationState | null)?.registrationData;
  const [otp, setOtp] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");

  if (!registrationData) {
    return <Navigate to="/register" replace />;
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError("");

    try {
      const response = await fetch(VERIFY_REGISTER_API_URL, {
        method: "POST",
        credentials: "include",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          ...registrationData,
          otp,
        }),
      });

      const payload = await response.json().catch(() => null);

      if (!response.ok) {
        setError(getApiErrorMessage(payload));
        return;
      }

      storeAuthSession(unwrapApiResponse<AuthSessionResponse>(payload));
      navigate("/dashboard", { replace: true });
    } catch {
      setError("تعذر الاتصال بالخدمة. حاول مرة أخرى بعد قليل.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <AuthLayout
      badge="تأكيد البريد"
      title="أدخل رمز التحقق"
      subtitle={`أرسلنا رمز تحقق إلى ${registrationData.email}. أدخل الأرقام الستة لإكمال إنشاء الحساب.`}
      footer={
        <>
          البريد غير صحيح؟{" "}
          <Link to="/register" className="text-teal font-semibold hover:underline">
            تعديل البيانات
          </Link>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-5">
        {error && (
          <div className="rounded-xl border border-danger/30 bg-danger-soft p-3 text-xs leading-relaxed text-danger">
            {error}
          </div>
        )}

        <div className="flex justify-center">
          <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
            <MailCheck className="mx-auto mb-3 h-6 w-6 text-teal" />
            <InputOTP
              maxLength={6}
              value={otp}
              onChange={(value) => {
                setOtp(value);
                setError("");
              }}
              containerClassName="justify-center gap-2"
            >
              <InputOTPGroup className="flex-row-reverse gap-2">
                {Array.from({ length: 6 }).map((_, index) => (
                  <InputOTPSlot
                    key={index}
                    index={index}
                    className="h-12 w-11 rounded-xl border border-border/70 bg-background text-lg font-bold text-navy first:rounded-xl first:border last:rounded-xl"
                  />
                ))}
              </InputOTPGroup>
            </InputOTP>
          </div>
        </div>

        <Button
          type="submit"
          size="lg"
          disabled={isSubmitting || otp.length !== 6}
          className="h-11 w-full rounded-xl bg-gradient-brand text-base shadow-glow hover:opacity-90"
        >
          {isSubmitting ? (
            <Loader2 className="ml-1 h-4 w-4 animate-spin" />
          ) : (
            <ArrowLeft className="ml-1 h-4 w-4" />
          )}
          {isSubmitting ? "جاري التحقق..." : "تحقق"}
        </Button>
      </form>
    </AuthLayout>
  );
}
