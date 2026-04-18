-- ============================================================
-- HUZUR MOBİLYA - SUPABASE VERİTABANI ŞEMASI
-- Bu dosyayı Supabase SQL Editor'de çalıştırın
-- ============================================================

-- ============================================================
-- 1) EXTENSION'LAR
-- ============================================================
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================================
-- 2) ENUM TİPLERİ
-- ============================================================
CREATE TYPE user_role AS ENUM ('admin', 'manager', 'employee');
CREATE TYPE order_status AS ENUM ('beklemede', 'hazirlaniyor', 'kargoda', 'teslim_edildi', 'iptal');
CREATE TYPE payment_status AS ENUM ('odenmedi', 'kismi_odendi', 'odendi', 'iade');
CREATE TYPE stock_movement_type AS ENUM ('giris', 'cikis', 'transfer', 'iade', 'fire');
CREATE TYPE employee_status AS ENUM ('aktif', 'izinli', 'pasif', 'ayrildi');
CREATE TYPE leave_type AS ENUM ('yillik', 'mazeret', 'hastalik', 'dogum', 'ucretsiz');
CREATE TYPE leave_status AS ENUM ('beklemede', 'onaylandi', 'reddedildi');

-- ============================================================
-- 3) KULLANICILAR / PROFILLER
-- ============================================================
CREATE TABLE profiles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    auth_user_id UUID UNIQUE,
    email TEXT UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    full_name TEXT NOT NULL,
    phone TEXT,
    role user_role NOT NULL DEFAULT 'employee',
    avatar_url TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    last_login TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_profiles_email ON profiles(email);
CREATE INDEX idx_profiles_role ON profiles(role);

-- ============================================================
-- 4) KATEGORİLER
-- ============================================================
CREATE TABLE categories (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    description TEXT,
    parent_id UUID REFERENCES categories(id) ON DELETE SET NULL,
    icon_name TEXT,
    sort_order INT DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_categories_parent ON categories(parent_id);

-- ============================================================
-- 5) TEDARİKÇİLER
-- ============================================================
CREATE TABLE suppliers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_name TEXT NOT NULL,
    contact_name TEXT,
    email TEXT,
    phone TEXT,
    address TEXT,
    city TEXT,
    tax_number TEXT,
    tax_office TEXT,
    notes TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_suppliers_company ON suppliers(company_name);

-- ============================================================
-- 6) DEPOLAR
-- ============================================================
CREATE TABLE warehouses (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    address TEXT,
    city TEXT,
    manager_id UUID REFERENCES profiles(id) ON DELETE SET NULL,
    capacity INT DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ============================================================
-- 7) ÜRÜNLER
-- ============================================================
CREATE TABLE products (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    sku TEXT UNIQUE NOT NULL,
    barcode TEXT UNIQUE,
    name TEXT NOT NULL,
    description TEXT,
    category_id UUID REFERENCES categories(id) ON DELETE SET NULL,
    supplier_id UUID REFERENCES suppliers(id) ON DELETE SET NULL,
    purchase_price DECIMAL(12,2) NOT NULL DEFAULT 0,
    sale_price DECIMAL(12,2) NOT NULL DEFAULT 0,
    tax_rate DECIMAL(5,2) NOT NULL DEFAULT 18.00,
    weight DECIMAL(10,2),
    dimensions TEXT,
    color TEXT,
    material TEXT,
    image_url TEXT,
    min_stock_level INT NOT NULL DEFAULT 5,
    max_stock_level INT DEFAULT 1000,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_products_sku ON products(sku);
CREATE INDEX idx_products_barcode ON products(barcode);
CREATE INDEX idx_products_category ON products(category_id);
CREATE INDEX idx_products_supplier ON products(supplier_id);
CREATE INDEX idx_products_name ON products(name);

-- ============================================================
-- 8) STOK
-- ============================================================
CREATE TABLE stock (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    warehouse_id UUID NOT NULL REFERENCES warehouses(id) ON DELETE CASCADE,
    quantity INT NOT NULL DEFAULT 0,
    reserved_quantity INT NOT NULL DEFAULT 0,
    last_counted_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(product_id, warehouse_id)
);

CREATE INDEX idx_stock_product ON stock(product_id);
CREATE INDEX idx_stock_warehouse ON stock(warehouse_id);

-- ============================================================
-- 9) STOK HAREKETLERİ
-- ============================================================
CREATE TABLE stock_movements (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    warehouse_id UUID NOT NULL REFERENCES warehouses(id) ON DELETE CASCADE,
    movement_type stock_movement_type NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(12,2),
    reference_no TEXT,
    notes TEXT,
    created_by UUID REFERENCES profiles(id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_stock_movements_product ON stock_movements(product_id);
CREATE INDEX idx_stock_movements_warehouse ON stock_movements(warehouse_id);
CREATE INDEX idx_stock_movements_type ON stock_movements(movement_type);
CREATE INDEX idx_stock_movements_date ON stock_movements(created_at);

-- ============================================================
-- 10) MÜŞTERİLER
-- ============================================================
CREATE TABLE customers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    full_name TEXT NOT NULL,
    email TEXT,
    phone TEXT,
    address TEXT,
    city TEXT,
    tax_number TEXT,
    tax_office TEXT,
    notes TEXT,
    total_orders INT DEFAULT 0,
    total_spent DECIMAL(14,2) DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_customers_name ON customers(full_name);
CREATE INDEX idx_customers_phone ON customers(phone);

-- ============================================================
-- 11) SİPARİŞLER
-- ============================================================
CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_no TEXT UNIQUE NOT NULL,
    customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
    order_status order_status NOT NULL DEFAULT 'beklemede',
    payment_status payment_status NOT NULL DEFAULT 'odenmedi',
    subtotal DECIMAL(14,2) NOT NULL DEFAULT 0,
    tax_total DECIMAL(14,2) NOT NULL DEFAULT 0,
    discount_total DECIMAL(14,2) NOT NULL DEFAULT 0,
    grand_total DECIMAL(14,2) NOT NULL DEFAULT 0,
    shipping_address TEXT,
    shipping_city TEXT,
    notes TEXT,
    created_by UUID REFERENCES profiles(id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_orders_no ON orders(order_no);
CREATE INDEX idx_orders_customer ON orders(customer_id);
CREATE INDEX idx_orders_status ON orders(order_status);
CREATE INDEX idx_orders_date ON orders(created_at);

-- ============================================================
-- 12) SİPARİŞ DETAYLARI
-- ============================================================
CREATE TABLE order_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_id UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    quantity INT NOT NULL DEFAULT 1,
    unit_price DECIMAL(12,2) NOT NULL,
    tax_rate DECIMAL(5,2) NOT NULL DEFAULT 18.00,
    discount_rate DECIMAL(5,2) DEFAULT 0,
    line_total DECIMAL(14,2) NOT NULL,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_order_items_order ON order_items(order_id);
CREATE INDEX idx_order_items_product ON order_items(product_id);

-- ============================================================
-- 13) ÇALIŞANLAR (EK BİLGİLER)
-- ============================================================
CREATE TABLE employees (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    profile_id UUID UNIQUE NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
    employee_no TEXT UNIQUE NOT NULL,
    department TEXT NOT NULL,
    position TEXT NOT NULL,
    hire_date DATE NOT NULL,
    birth_date DATE,
    national_id TEXT,
    address TEXT,
    city TEXT,
    emergency_contact TEXT,
    emergency_phone TEXT,
    salary DECIMAL(12,2),
    status employee_status NOT NULL DEFAULT 'aktif',
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_employees_no ON employees(employee_no);
CREATE INDEX idx_employees_department ON employees(department);
CREATE INDEX idx_employees_status ON employees(status);

-- ============================================================
-- 14) İZİN TALEPLERİ
-- ============================================================
CREATE TABLE leave_requests (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    employee_id UUID NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
    leave_type leave_type NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    total_days INT NOT NULL,
    reason TEXT,
    status leave_status NOT NULL DEFAULT 'beklemede',
    approved_by UUID REFERENCES profiles(id) ON DELETE SET NULL,
    approved_at TIMESTAMPTZ,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_leave_employee ON leave_requests(employee_id);
CREATE INDEX idx_leave_status ON leave_requests(status);

-- ============================================================
-- 15) MAAŞ ÖDEMELERİ
-- ============================================================
CREATE TABLE salary_payments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    employee_id UUID NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
    payment_period TEXT NOT NULL,
    base_salary DECIMAL(12,2) NOT NULL,
    bonus DECIMAL(12,2) DEFAULT 0,
    deductions DECIMAL(12,2) DEFAULT 0,
    net_salary DECIMAL(12,2) NOT NULL,
    payment_date DATE,
    is_paid BOOLEAN NOT NULL DEFAULT FALSE,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_salary_employee ON salary_payments(employee_id);
CREATE INDEX idx_salary_period ON salary_payments(payment_period);

-- ============================================================
-- 16) GÖREVLER
-- ============================================================
CREATE TABLE tasks (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title TEXT NOT NULL,
    description TEXT,
    assigned_to UUID REFERENCES employees(id) ON DELETE SET NULL,
    assigned_by UUID REFERENCES profiles(id) ON DELETE SET NULL,
    due_date DATE,
    priority INT DEFAULT 2,
    is_completed BOOLEAN NOT NULL DEFAULT FALSE,
    completed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_tasks_assigned ON tasks(assigned_to);
CREATE INDEX idx_tasks_due ON tasks(due_date);

-- ============================================================
-- 17) AKTİVİTE LOGLARI
-- ============================================================
CREATE TABLE activity_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES profiles(id) ON DELETE SET NULL,
    action TEXT NOT NULL,
    entity_type TEXT NOT NULL,
    entity_id UUID,
    details JSONB,
    ip_address TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_activity_user ON activity_logs(user_id);
CREATE INDEX idx_activity_entity ON activity_logs(entity_type, entity_id);
CREATE INDEX idx_activity_date ON activity_logs(created_at);

-- ============================================================
-- 18) BİLDİRİMLER
-- ============================================================
CREATE TABLE notifications (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
    title TEXT NOT NULL,
    message TEXT NOT NULL,
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    link TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_notifications_user ON notifications(user_id);
CREATE INDEX idx_notifications_read ON notifications(is_read);

-- ============================================================
-- 19) UPDATED_AT TRİGGER FONKSİYONU
-- ============================================================
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Tüm tablolara trigger uygula
DO $$
DECLARE
    t TEXT;
BEGIN
    FOR t IN
        SELECT unnest(ARRAY[
            'profiles','categories','suppliers','warehouses','products',
            'stock','customers','orders','employees','leave_requests','tasks'
        ])
    LOOP
        EXECUTE format('
            CREATE TRIGGER set_updated_at
            BEFORE UPDATE ON %I
            FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
        ', t);
    END LOOP;
END;
$$;

-- ============================================================
-- 20) STOK OTOMATİK GÜNCELLEME TRİGGER
-- ============================================================
CREATE OR REPLACE FUNCTION update_stock_on_movement()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.movement_type IN ('giris', 'iade') THEN
        INSERT INTO stock (product_id, warehouse_id, quantity)
        VALUES (NEW.product_id, NEW.warehouse_id, NEW.quantity)
        ON CONFLICT (product_id, warehouse_id)
        DO UPDATE SET quantity = stock.quantity + NEW.quantity,
                      updated_at = NOW();
    ELSIF NEW.movement_type IN ('cikis', 'fire') THEN
        UPDATE stock
        SET quantity = quantity - NEW.quantity, updated_at = NOW()
        WHERE product_id = NEW.product_id AND warehouse_id = NEW.warehouse_id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_stock_movement
AFTER INSERT ON stock_movements
FOR EACH ROW EXECUTE FUNCTION update_stock_on_movement();

-- ============================================================
-- 21) DÜŞÜK STOK BİLDİRİM TRİGGER
-- ============================================================
CREATE OR REPLACE FUNCTION check_low_stock()
RETURNS TRIGGER AS $$
DECLARE
    min_level INT;
    product_name TEXT;
    admin_ids UUID[];
BEGIN
    SELECT p.min_stock_level, p.name INTO min_level, product_name
    FROM products p WHERE p.id = NEW.product_id;

    IF NEW.quantity <= min_level THEN
        SELECT array_agg(id) INTO admin_ids
        FROM profiles WHERE role IN ('admin','manager') AND is_active = TRUE;

        IF admin_ids IS NOT NULL THEN
            INSERT INTO notifications (user_id, title, message)
            SELECT unnest(admin_ids),
                   'Düşük Stok Uyarısı',
                   product_name || ' ürününün stoğu ' || NEW.quantity || ' adede düştü!';
        END IF;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_low_stock_alert
AFTER UPDATE OF quantity ON stock
FOR EACH ROW EXECUTE FUNCTION check_low_stock();

-- ============================================================
-- 22) SİPARİŞ NUMARASI OTOMATİK ÜRETME
-- ============================================================
CREATE OR REPLACE FUNCTION generate_order_no()
RETURNS TRIGGER AS $$
DECLARE
    next_no INT;
BEGIN
    SELECT COALESCE(MAX(CAST(SUBSTRING(order_no FROM 9) AS INT)), 0) + 1
    INTO next_no
    FROM orders
    WHERE order_no LIKE 'HM-' || TO_CHAR(NOW(), 'YYMM') || '%';

    NEW.order_no := 'HM-' || TO_CHAR(NOW(), 'YYMM') || '-' || LPAD(next_no::TEXT, 4, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_order_no
BEFORE INSERT ON orders
FOR EACH ROW
WHEN (NEW.order_no IS NULL OR NEW.order_no = '')
EXECUTE FUNCTION generate_order_no();

-- ============================================================
-- 23) ÇALIŞAN NUMARASI OTOMATİK ÜRETME
-- ============================================================
CREATE OR REPLACE FUNCTION generate_employee_no()
RETURNS TRIGGER AS $$
DECLARE
    next_no INT;
BEGIN
    SELECT COALESCE(MAX(CAST(SUBSTRING(employee_no FROM 4) AS INT)), 0) + 1
    INTO next_no FROM employees;
    NEW.employee_no := 'HM-' || LPAD(next_no::TEXT, 4, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_employee_no
BEFORE INSERT ON employees
FOR EACH ROW
WHEN (NEW.employee_no IS NULL OR NEW.employee_no = '')
EXECUTE FUNCTION generate_employee_no();

-- ============================================================
-- 24) VIEW'LAR
-- ============================================================

-- Stok özet görünümü
CREATE VIEW vw_stock_summary AS
SELECT
    p.id AS product_id, p.sku, p.name AS product_name,
    c.name AS category_name, s2.company_name AS supplier_name,
    p.purchase_price, p.sale_price,
    COALESCE(SUM(st.quantity), 0) AS total_stock,
    COALESCE(SUM(st.reserved_quantity), 0) AS total_reserved,
    p.min_stock_level,
    CASE WHEN COALESCE(SUM(st.quantity), 0) <= p.min_stock_level THEN TRUE ELSE FALSE END AS is_low_stock
FROM products p
LEFT JOIN stock st ON st.product_id = p.id
LEFT JOIN categories c ON c.id = p.category_id
LEFT JOIN suppliers s2 ON s2.id = p.supplier_id
WHERE p.is_active = TRUE
GROUP BY p.id, p.sku, p.name, c.name, s2.company_name, p.purchase_price, p.sale_price, p.min_stock_level;

-- Çalışan özet görünümü
CREATE VIEW vw_employee_summary AS
SELECT
    e.id, e.employee_no, pr.full_name, pr.email, pr.phone,
    e.department, e.position, e.hire_date, e.status, e.salary,
    (SELECT COUNT(*) FROM leave_requests lr WHERE lr.employee_id = e.id AND lr.status = 'onaylandi'
     AND EXTRACT(YEAR FROM lr.start_date) = EXTRACT(YEAR FROM NOW())) AS leaves_this_year,
    (SELECT COUNT(*) FROM tasks t WHERE t.assigned_to = e.id AND t.is_completed = FALSE) AS pending_tasks
FROM employees e
JOIN profiles pr ON pr.id = e.profile_id;

-- Sipariş özet görünümü
CREATE VIEW vw_order_summary AS
SELECT
    o.id, o.order_no, cu.full_name AS customer_name, cu.phone AS customer_phone,
    o.order_status, o.payment_status, o.grand_total,
    o.created_at, pr.full_name AS created_by_name,
    (SELECT COUNT(*) FROM order_items oi WHERE oi.order_id = o.id) AS item_count
FROM orders o
LEFT JOIN customers cu ON cu.id = o.customer_id
LEFT JOIN profiles pr ON pr.id = o.created_by;

-- Dashboard istatistikleri
CREATE VIEW vw_dashboard_stats AS
SELECT
    (SELECT COUNT(*) FROM products WHERE is_active = TRUE) AS total_products,
    (SELECT COUNT(*) FROM products p2 JOIN stock s ON s.product_id = p2.id WHERE s.quantity <= p2.min_stock_level AND p2.is_active = TRUE) AS low_stock_products,
    (SELECT COUNT(*) FROM employees WHERE status = 'aktif') AS active_employees,
    (SELECT COUNT(*) FROM orders WHERE created_at >= DATE_TRUNC('month', NOW())) AS monthly_orders,
    (SELECT COALESCE(SUM(grand_total), 0) FROM orders WHERE created_at >= DATE_TRUNC('month', NOW()) AND payment_status != 'iade') AS monthly_revenue,
    (SELECT COUNT(*) FROM orders WHERE order_status = 'beklemede') AS pending_orders,
    (SELECT COUNT(*) FROM customers WHERE is_active = TRUE) AS total_customers,
    (SELECT COUNT(*) FROM tasks WHERE is_completed = FALSE) AS pending_tasks;

-- ============================================================
-- 25) ROW LEVEL SECURITY (RLS)
-- ============================================================

-- ── PROFILES ──
ALTER TABLE profiles ENABLE ROW LEVEL SECURITY;
CREATE POLICY "profiles_select" ON profiles FOR SELECT USING (TRUE);
CREATE POLICY "profiles_insert" ON profiles FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "profiles_update" ON profiles FOR UPDATE USING (TRUE);
CREATE POLICY "profiles_delete" ON profiles FOR DELETE USING (TRUE);

-- ── CATEGORIES ──
ALTER TABLE categories ENABLE ROW LEVEL SECURITY;
CREATE POLICY "categories_select" ON categories FOR SELECT USING (TRUE);
CREATE POLICY "categories_insert" ON categories FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "categories_update" ON categories FOR UPDATE USING (TRUE);
CREATE POLICY "categories_delete" ON categories FOR DELETE USING (TRUE);

-- ── SUPPLIERS ──
ALTER TABLE suppliers ENABLE ROW LEVEL SECURITY;
CREATE POLICY "suppliers_select" ON suppliers FOR SELECT USING (TRUE);
CREATE POLICY "suppliers_insert" ON suppliers FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "suppliers_update" ON suppliers FOR UPDATE USING (TRUE);
CREATE POLICY "suppliers_delete" ON suppliers FOR DELETE USING (TRUE);

-- ── WAREHOUSES ──
ALTER TABLE warehouses ENABLE ROW LEVEL SECURITY;
CREATE POLICY "warehouses_select" ON warehouses FOR SELECT USING (TRUE);
CREATE POLICY "warehouses_insert" ON warehouses FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "warehouses_update" ON warehouses FOR UPDATE USING (TRUE);
CREATE POLICY "warehouses_delete" ON warehouses FOR DELETE USING (TRUE);

-- ── PRODUCTS ──
ALTER TABLE products ENABLE ROW LEVEL SECURITY;
CREATE POLICY "products_select" ON products FOR SELECT USING (TRUE);
CREATE POLICY "products_insert" ON products FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "products_update" ON products FOR UPDATE USING (TRUE);
CREATE POLICY "products_delete" ON products FOR DELETE USING (TRUE);

-- ── STOCK ──
ALTER TABLE stock ENABLE ROW LEVEL SECURITY;
CREATE POLICY "stock_select" ON stock FOR SELECT USING (TRUE);
CREATE POLICY "stock_insert" ON stock FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "stock_update" ON stock FOR UPDATE USING (TRUE);
CREATE POLICY "stock_delete" ON stock FOR DELETE USING (TRUE);

-- ── STOCK_MOVEMENTS ──
ALTER TABLE stock_movements ENABLE ROW LEVEL SECURITY;
CREATE POLICY "stock_movements_select" ON stock_movements FOR SELECT USING (TRUE);
CREATE POLICY "stock_movements_insert" ON stock_movements FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "stock_movements_update" ON stock_movements FOR UPDATE USING (TRUE);
CREATE POLICY "stock_movements_delete" ON stock_movements FOR DELETE USING (TRUE);

-- ── CUSTOMERS ──
ALTER TABLE customers ENABLE ROW LEVEL SECURITY;
CREATE POLICY "customers_select" ON customers FOR SELECT USING (TRUE);
CREATE POLICY "customers_insert" ON customers FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "customers_update" ON customers FOR UPDATE USING (TRUE);
CREATE POLICY "customers_delete" ON customers FOR DELETE USING (TRUE);

-- ── ORDERS ──
ALTER TABLE orders ENABLE ROW LEVEL SECURITY;
CREATE POLICY "orders_select" ON orders FOR SELECT USING (TRUE);
CREATE POLICY "orders_insert" ON orders FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "orders_update" ON orders FOR UPDATE USING (TRUE);
CREATE POLICY "orders_delete" ON orders FOR DELETE USING (TRUE);

-- ── ORDER_ITEMS ──
ALTER TABLE order_items ENABLE ROW LEVEL SECURITY;
CREATE POLICY "order_items_select" ON order_items FOR SELECT USING (TRUE);
CREATE POLICY "order_items_insert" ON order_items FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "order_items_update" ON order_items FOR UPDATE USING (TRUE);
CREATE POLICY "order_items_delete" ON order_items FOR DELETE USING (TRUE);

-- ── EMPLOYEES ──
ALTER TABLE employees ENABLE ROW LEVEL SECURITY;
CREATE POLICY "employees_select" ON employees FOR SELECT USING (TRUE);
CREATE POLICY "employees_insert" ON employees FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "employees_update" ON employees FOR UPDATE USING (TRUE);
CREATE POLICY "employees_delete" ON employees FOR DELETE USING (TRUE);

-- ── LEAVE_REQUESTS ──
ALTER TABLE leave_requests ENABLE ROW LEVEL SECURITY;
CREATE POLICY "leave_requests_select" ON leave_requests FOR SELECT USING (TRUE);
CREATE POLICY "leave_requests_insert" ON leave_requests FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "leave_requests_update" ON leave_requests FOR UPDATE USING (TRUE);
CREATE POLICY "leave_requests_delete" ON leave_requests FOR DELETE USING (TRUE);

-- ── SALARY_PAYMENTS ──
ALTER TABLE salary_payments ENABLE ROW LEVEL SECURITY;
CREATE POLICY "salary_payments_select" ON salary_payments FOR SELECT USING (TRUE);
CREATE POLICY "salary_payments_insert" ON salary_payments FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "salary_payments_update" ON salary_payments FOR UPDATE USING (TRUE);
CREATE POLICY "salary_payments_delete" ON salary_payments FOR DELETE USING (TRUE);

-- ── TASKS ──
ALTER TABLE tasks ENABLE ROW LEVEL SECURITY;
CREATE POLICY "tasks_select" ON tasks FOR SELECT USING (TRUE);
CREATE POLICY "tasks_insert" ON tasks FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "tasks_update" ON tasks FOR UPDATE USING (TRUE);
CREATE POLICY "tasks_delete" ON tasks FOR DELETE USING (TRUE);

-- ── ACTIVITY_LOGS ──
ALTER TABLE activity_logs ENABLE ROW LEVEL SECURITY;
CREATE POLICY "activity_logs_select" ON activity_logs FOR SELECT USING (TRUE);
CREATE POLICY "activity_logs_insert" ON activity_logs FOR INSERT WITH CHECK (TRUE);

-- ── NOTIFICATIONS ──
ALTER TABLE notifications ENABLE ROW LEVEL SECURITY;
CREATE POLICY "notifications_select" ON notifications FOR SELECT USING (TRUE);
CREATE POLICY "notifications_insert" ON notifications FOR INSERT WITH CHECK (TRUE);
CREATE POLICY "notifications_update" ON notifications FOR UPDATE USING (TRUE);
CREATE POLICY "notifications_delete" ON notifications FOR DELETE USING (TRUE);

-- ============================================================
-- 26) BAŞLANGIÇ VERİLERİ
-- ============================================================
INSERT INTO profiles (email, password_hash, full_name, phone, role) VALUES
('admin@huzurmobilya.com', 'CHANGE_ME_USE_BCRYPT', 'Sistem Yöneticisi', '0555 000 0000', 'admin');

INSERT INTO categories (name, description, sort_order) VALUES
('Koltuk Takımları', 'Oturma grubu ve koltuk takımları', 1),
('Yatak Odası', 'Yatak odası mobilyaları', 2),
('Yemek Odası', 'Yemek odası takımları ve sandalyeler', 3),
('Ofis Mobilyası', 'Ofis masaları, koltukları ve dolaplar', 4),
('Genç Odası', 'Genç ve çocuk odası mobilyaları', 5),
('Bahçe Mobilyası', 'Dış mekan mobilyaları', 6),
('Aksesuar', 'Dekoratif ürünler ve aksesuarlar', 7);

INSERT INTO warehouses (name, address, city, capacity) VALUES
('Ana Depo', 'Organize Sanayi Bölgesi No:15', 'İstanbul', 5000),
('Mağaza Deposu', 'Atatürk Cad. No:42', 'İstanbul', 500),
('Ankara Depo', 'İvedik OSB No:8', 'Ankara', 3000);

-- ============================================================
-- TAMAMLANDI!
-- ============================================================
