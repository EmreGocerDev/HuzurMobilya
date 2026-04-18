-- ============================================================
-- Huzur Mobilya - Supabase Storage Kurulum Scripti
-- Supabase Dashboard > SQL Editor'de çalıştırın
-- ============================================================

-- 1. product-images bucket oluştur (herkese açık)
INSERT INTO storage.buckets (id, name, public, file_size_limit, allowed_mime_types)
VALUES (
    'product-images',
    'product-images',
    true,
    5242880,  -- 5 MB limit
    ARRAY['image/jpeg', 'image/png', 'image/webp', 'image/gif']
)
ON CONFLICT (id) DO UPDATE SET
    public = true,
    file_size_limit = 5242880,
    allowed_mime_types = ARRAY['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

-- 2. Storage RLS politikaları
-- Herkes okuyabilir (public bucket)
DROP POLICY IF EXISTS "product_images_select" ON storage.objects;
CREATE POLICY "product_images_select" ON storage.objects
    FOR SELECT USING (bucket_id = 'product-images');

-- Herkes yükleyebilir (anon key ile)
DROP POLICY IF EXISTS "product_images_insert" ON storage.objects;
CREATE POLICY "product_images_insert" ON storage.objects
    FOR INSERT WITH CHECK (bucket_id = 'product-images');

-- Herkes güncelleyebilir
DROP POLICY IF EXISTS "product_images_update" ON storage.objects;
CREATE POLICY "product_images_update" ON storage.objects
    FOR UPDATE USING (bucket_id = 'product-images');

-- Herkes silebilir
DROP POLICY IF EXISTS "product_images_delete" ON storage.objects;
CREATE POLICY "product_images_delete" ON storage.objects
    FOR DELETE USING (bucket_id = 'product-images');
