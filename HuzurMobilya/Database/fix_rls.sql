-- ============================================================
-- HUZUR MOBİLYA - RLS DÜZELTME SCRIPTI
-- Mevcut veritabanında RLS sorunlarını düzeltmek için
-- Supabase SQL Editor'de çalıştırın
-- ============================================================

-- Önce mevcut policy'leri kaldır
DO $$
DECLARE
    pol RECORD;
BEGIN
    FOR pol IN
        SELECT policyname, tablename
        FROM pg_policies
        WHERE schemaname = 'public'
    LOOP
        EXECUTE format('DROP POLICY IF EXISTS %I ON %I', pol.policyname, pol.tablename);
    END LOOP;
END;
$$;

-- Tüm tablolarda RLS'yi etkinleştir ve permissive policy ekle
DO $$
DECLARE
    t TEXT;
BEGIN
    FOR t IN
        SELECT unnest(ARRAY[
            'profiles','categories','suppliers','warehouses','products',
            'stock','stock_movements','customers','orders','order_items',
            'employees','leave_requests','salary_payments','tasks',
            'activity_logs','notifications'
        ])
    LOOP
        EXECUTE format('ALTER TABLE %I ENABLE ROW LEVEL SECURITY', t);
        EXECUTE format('CREATE POLICY %I ON %I FOR SELECT USING (TRUE)', t || '_select', t);
        EXECUTE format('CREATE POLICY %I ON %I FOR INSERT WITH CHECK (TRUE)', t || '_insert', t);
        EXECUTE format('CREATE POLICY %I ON %I FOR UPDATE USING (TRUE)', t || '_update', t);
        EXECUTE format('CREATE POLICY %I ON %I FOR DELETE USING (TRUE)', t || '_delete', t);
    END LOOP;
END;
$$;

-- Doğrulama: Tüm policy'leri listele
SELECT tablename, policyname, cmd, qual FROM pg_policies WHERE schemaname = 'public' ORDER BY tablename;
