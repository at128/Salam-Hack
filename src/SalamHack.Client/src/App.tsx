import { useEffect, useState } from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Navigate, Outlet, Route, Routes } from "react-router-dom";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { Toaster } from "@/components/ui/toaster";
import { TooltipProvider } from "@/components/ui/tooltip";
import Index from "./pages/Index.tsx";
import Dashboard from "./pages/Dashboard.tsx";
import NotFound from "./pages/NotFound.tsx";
import Login from "./pages/Login.tsx";
import Register from "./pages/Register.tsx";
import DashboardLayout from "./components/dashboard/DashboardLayout.tsx";
import InvoicesPage from "./pages/dashboard/Invoices.tsx";
import PaymentsPage from "./pages/dashboard/Payments.tsx";
import ProfitPage from "./pages/dashboard/Profit.tsx";
import BreakdownPage from "./pages/dashboard/Breakdown.tsx";
import CashflowPage from "./pages/dashboard/Cashflow.tsx";
import PricingPage from "./pages/dashboard/Pricing.tsx";
import ServicesPage from "./pages/dashboard/Services.tsx";
import AiAnalyzerPage from "./pages/dashboard/AiAnalyzer.tsx";
import ClientRiskAnalyzerPage from "./pages/dashboard/ClientRiskAnalyzer.tsx";
import CustomersPage from "./pages/dashboard/Customers.tsx";
import ProjectsPage from "./pages/dashboard/Projects.tsx";
import ProfilePage from "./pages/dashboard/Profile.tsx";
import ChangePasswordPage from "./pages/dashboard/ChangePassword.tsx";
import { isAccessTokenExpired, isAuthenticated, refreshAccessToken } from "./lib/auth.ts";

const queryClient = new QueryClient();

function RequireAuth() {
  const [status, setStatus] = useState<"checking" | "authenticated" | "guest">("checking");

  useEffect(() => {
    let active = true;

    async function verifySession() {
      if (!isAuthenticated()) {
        if (active) setStatus("guest");
        return;
      }

      if (isAccessTokenExpired()) {
        const refreshed = await refreshAccessToken();
        if (active) setStatus(refreshed ? "authenticated" : "guest");
        return;
      }

      if (active) setStatus("authenticated");
    }

    void verifySession();

    return () => {
      active = false;
    };
  }, []);

  if (status === "checking") return null;
  return status === "authenticated" ? <Outlet /> : <Navigate to="/login" replace />;
}

const App = () => (
  <QueryClientProvider client={queryClient}>
    <TooltipProvider>
      <Toaster />
      <Sonner />
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Index />} />
          <Route element={<RequireAuth />}>
            <Route path="/dashboard" element={<DashboardLayout />}>
              <Route index element={<Dashboard />} />
              <Route path="services" element={<ServicesPage />} />
              <Route path="customers" element={<CustomersPage />} />
              <Route path="projects" element={<ProjectsPage />} />
              <Route path="invoices" element={<InvoicesPage />} />
              <Route path="payments" element={<PaymentsPage />} />
              <Route path="profit" element={<ProfitPage />} />
              <Route path="breakdown" element={<BreakdownPage />} />
              <Route path="cashflow" element={<CashflowPage />} />
              <Route path="pricing" element={<PricingPage />} />
              <Route path="ai" element={<AiAnalyzerPage />} />
              <Route path="client-risk" element={<ClientRiskAnalyzerPage />} />
              <Route path="profile" element={<ProfilePage />} />
              <Route path="change-password" element={<ChangePasswordPage />} />
            </Route>
          </Route>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          {/* ADD ALL CUSTOM ROUTES ABOVE THE CATCH-ALL "*" ROUTE */}
          <Route path="*" element={<NotFound />} />
        </Routes>
      </BrowserRouter>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;
