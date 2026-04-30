import { useEffect, useState } from "react";
import { ArrowRight, Download, Loader2, Printer } from "lucide-react";
import { useNavigate, useParams } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { getApiErrorMessage, getValidAccessToken, unwrapApiResponse } from "@/lib/auth";
import {
  InvoicePrintPreview,
  InvoicePrintStyles,
  type InvoiceDto,
} from "@/components/dashboard/InvoicesTable";

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const INVOICES_API_URL = `${API_BASE_URL}/api/v1/invoices`;

async function fetchInvoice(invoiceId: string): Promise<InvoiceDto> {
  const token = await getValidAccessToken();
  if (!token) throw new Error("Missing access token.");

  const response = await fetch(`${INVOICES_API_URL}/${invoiceId}`, {
    method: "GET",
    headers: {
      Accept: "application/json",
      Authorization: `Bearer ${token}`,
    },
  });

  const payload = await response.json().catch(() => null);
  if (!response.ok) throw new Error(getApiErrorMessage(payload, "تعذر تحميل الفاتورة."));

  return unwrapApiResponse<InvoiceDto>(payload);
}

export default function InvoiceDetailsPage() {
  const navigate = useNavigate();
  const { invoiceId } = useParams();
  const [invoice, setInvoice] = useState<InvoiceDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isDownloading, setIsDownloading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;

    async function loadInvoice() {
      if (!invoiceId) {
        setError("معرف الفاتورة غير موجود.");
        setIsLoading(false);
        return;
      }

      setIsLoading(true);
      setError("");

      try {
        const result = await fetchInvoice(invoiceId);
        if (active) setInvoice(result);
      } catch (err) {
        if (active) setError(err instanceof Error ? err.message : "تعذر تحميل الفاتورة.");
      } finally {
        if (active) setIsLoading(false);
      }
    }

    void loadInvoice();

    return () => {
      active = false;
    };
  }, [invoiceId]);

  const downloadPdf = async () => {
    if (!invoice) return;

    const element = document.querySelector<HTMLElement>(".invoice-print-area");
    if (!element) {
      setError("تعذر العثور على محتوى الفاتورة.");
      return;
    }

    setIsDownloading(true);
    setError("");

    try {
      const [{ default: html2canvas }, { default: jsPDF }] = await Promise.all([
        import("html2canvas"),
        import("jspdf"),
      ]);

      const canvas = await html2canvas(element, {
        scale: 2,
        backgroundColor: "#ffffff",
        useCORS: true,
      });

      const pdf = new jsPDF("p", "mm", "a4");
      const pageWidth = pdf.internal.pageSize.getWidth();
      const pageHeight = pdf.internal.pageSize.getHeight();
      const imageWidth = pageWidth;
      const imageHeight = (canvas.height * imageWidth) / canvas.width;
      const imageData = canvas.toDataURL("image/png");

      let remainingHeight = imageHeight;
      let position = 0;

      pdf.addImage(imageData, "PNG", 0, position, imageWidth, imageHeight);
      remainingHeight -= pageHeight;

      while (remainingHeight > 0) {
        position -= pageHeight;
        pdf.addPage();
        pdf.addImage(imageData, "PNG", 0, position, imageWidth, imageHeight);
        remainingHeight -= pageHeight;
      }

      pdf.save(`invoice-${invoice.invoiceNumber || invoice.id}.pdf`);
    } catch {
      setError("تعذر تنزيل ملف PDF.");
    } finally {
      setIsDownloading(false);
    }
  };

  return (
    <>
      <InvoicePrintStyles />

      <div className="invoice-no-print flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-navy">عرض الفاتورة</h1>
          <p className="mt-1 text-sm text-muted-foreground">معاينة الفاتورة بنفس تنسيق الطباعة.</p>
        </div>

        <div className="flex flex-wrap gap-2">
          <Button type="button" variant="outline" className="rounded-xl" onClick={() => navigate("/dashboard/invoices")}>
            <ArrowRight className="h-4 w-4" />
            رجوع
          </Button>
          <Button type="button" variant="outline" className="rounded-xl" disabled={!invoice} onClick={() => window.print()}>
            <Printer className="h-4 w-4" />
            طباعة
          </Button>
          <Button
            type="button"
            className="rounded-xl bg-gradient-brand shadow-glow hover:opacity-90"
            disabled={!invoice || isDownloading}
            onClick={() => void downloadPdf()}
          >
            {isDownloading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Download className="h-4 w-4" />}
            حفظ PDF
          </Button>
        </div>
      </div>

      {error && (
        <div className="invoice-no-print rounded-xl border border-danger/30 bg-danger-soft p-3 text-sm text-danger">
          {error}
        </div>
      )}

      {isLoading ? (
        <div className="invoice-no-print flex items-center justify-center rounded-xl bg-muted/40 p-10 text-sm text-muted-foreground">
          <Loader2 className="ml-2 h-4 w-4 animate-spin text-teal" />
          جاري تحميل الفاتورة...
        </div>
      ) : invoice ? (
        <InvoicePrintPreview invoice={invoice} />
      ) : null}
    </>
  );
}
