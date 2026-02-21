-- #1 ~ DATABASE RESET --
USE master;
GO

IF DB_ID('pc_builder_ml') IS NOT NULL
BEGIN
    ALTER DATABASE pc_builder_ml SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE pc_builder_ml;
END
GO

CREATE DATABASE pc_builder_ml;
GO
USE pc_builder_ml;
GO

-- #2 ~ tables for compatibility between components --

-- Sockets --
CREATE TABLE socket (socket_id INT PRIMARY KEY, name VARCHAR(50));
INSERT INTO socket VALUES
(1,'AM4'),
(2,'AM5'),
(3,'LGA1200'),
(4,'LGA1700'),
(5,'LGA1151'),
(6,'TR4'),
(7,'sTRX4'),
(8,'LGA2066'),
(9,'LGA1851'),     
(10,'SP5'),      
(11,'sWRX8'),     
(12,'FP7'),      
(13,'BGA1744');   

-- Chipsets -- 
CREATE TABLE chipset (chipset_id INT PRIMARY KEY, name VARCHAR(50));
INSERT INTO chipset VALUES
(1,'A320'),
(2,'B450'),
(3,'X470'),
(4,'B550'),
(5,'X570'),
(6,'B650'),
(7,'X670'),
(8,'H410'),
(9,'B460'),
(10,'Z490'),
(11,'H610'),
(12,'B660'),
(13,'Z690'),
(14,'Z790'),
(15,'H670'),
(16,'B760'),
(17,'H770'),
(18,'A620'),
(19,'X670E'),
(20,'WRX80'),     
(21,'TRX50'),    
(22,'SP5-P'),    
(23,'C741'),     
(97, 'Intel Mobile SoC'),
(98, 'AMD Mobile SoC');

-- Form Factor for Motherboard --
CREATE TABLE motherboard_form_factor (form_factor_id INT PRIMARY KEY, name VARCHAR(50));
INSERT INTO motherboard_form_factor VALUES
(1,'E-ATX'),
(2,'ATX'),
(3,'Micro-ATX'),
(4,'Mini-ITX'),
(5,'Mini-DTX'),
(6,'XL-ATX'),
(8,'Laptop');

-- Type of Memory --
CREATE TABLE memory_type (memory_type_id INT PRIMARY KEY, name VARCHAR(50));
INSERT INTO memory_type VALUES
(1,'DDR3'),
(2,'DDR4'),
(3,'DDR5'),
(4,'LPDDR4'),
(5,'LPDDR5');

-- Network Interface --
CREATE TABLE network_interface (interface_id INT PRIMARY KEY, name VARCHAR(50));
INSERT INTO network_interface VALUES
(1,'PCIe'),
(2,'PCIe x1'),
(3,'PCIe x4'),
(4,'M.2'),
(5,'USB'),
(6,'Onboard');
GO

-- #3 ~ Tables for main components --

-- CPU --
CREATE TABLE cpu (
    id INT IDENTITY PRIMARY KEY,
    name VARCHAR(50),
    price_usd FLOAT,
    core_count INT,
    core_clock FLOAT,
    boost_clock FLOAT,
    tdp INT,
    socket_id INT,
    cpu_compute_score FLOAT,
    cpu_multicore_score FLOAT,
    cpu_efficiency_score FLOAT,
    arch VARCHAR(20),
    suffix VARCHAR(10),
    segment VARCHAR(20)
);

-- Motherboard --
CREATE TABLE motherboard (
    id INT IDENTITY PRIMARY KEY,
    name VARCHAR(50),
    price_usd FLOAT,
    performance_score FLOAT,
    socket_id INT,
    form_factor_id INT,
    chipset_id INT
);

-- Memory --
CREATE TABLE memory (
    ram_id INT IDENTITY PRIMARY KEY,
    model VARCHAR(50),
    price_usd FLOAT,
    speed_mts INT,
    module_count INT,
    capacity_gb INT,
    ram_bandwidth_score FLOAT,
    ram_capacity_score FLOAT
);

-- GPU --
CREATE TABLE videocard (
    gpu_id INT IDENTITY PRIMARY KEY,
    model VARCHAR(50),
    price_usd FLOAT,
    power_draw_w INT,
    gpu_compute_score FLOAT,
    gpu_power_efficiency FLOAT
);

-- Internal Storage --
CREATE TABLE internal_storage (
    id INT IDENTITY PRIMARY KEY,
    price_usd FLOAT,
    performance_score FLOAT,
    model_name VARCHAR(50),
    amount_gb FLOAT
);

-- PSU--
CREATE TABLE psu (
    psu_id INT IDENTITY PRIMARY KEY,
    price_usd FLOAT,
    wattage INT,
    model_name VARCHAR(50)
); 

-- Type of PC Case --
CREATE TABLE pc_case (
    id INT IDENTITY PRIMARY KEY,
    price_usd FLOAT,
    case_type VARCHAR(20)
);

-- Wireless Network Card --
CREATE TABLE wireless_network_card (
    nic_id INT IDENTITY PRIMARY KEY,
    price_usd FLOAT
);
GO

-- #4 ~ Tables for all pc builds  --

-- Build tables --
CREATE TABLE computer_build (
    build_id BIGINT IDENTITY PRIMARY KEY,
    total_price_usd FLOAT,
    total_performance_score FLOAT,
    cpu_id INT,
    motherboard_id INT,
    gpu_id INT NULL,
    ram_id INT,
    storage_id INT,
    psu_id INT,
    case_id INT,
    nic_id INT,
    has_gpu BIT,
    computer_type VARCHAR(20),
    form_factor_label VARCHAR(20),
    is_portable BIT,
    has_builtin_screen BIT,
    has_builtin_keyboard BIT
);
GO

-- #5 ~ Setup  --

-- Numbers helper --
;WITH n AS (
    SELECT TOP (500)
    ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) n
    FROM sys.objects a CROSS JOIN sys.objects b
)
SELECT n INTO #numbers FROM n;
GO

-- #6 ~ Populate components --

-- CPU --
INSERT INTO cpu
(name, price_usd, core_count, core_clock, boost_clock, tdp, socket_id,
 cpu_compute_score, cpu_multicore_score, cpu_efficiency_score,
 arch, suffix, segment)
VALUES
('Intel Core i9-14900K', 589, 24, 3.2, 6.0, 125, 4, NULL, NULL, NULL, 'RaptorLake', 'K', 'desktop'),
('Intel Core i7-14700K', 409, 20, 3.4, 5.6, 125, 4, NULL, NULL, NULL, 'RaptorLake', 'K', 'desktop'),
('Intel Core i5-14600K', 319, 14, 3.5, 5.3, 125, 4, NULL, NULL, NULL, 'RaptorLake', 'K', 'desktop'),
('Intel Core i5-14400F', 229, 10, 2.5, 4.7, 65, 4, NULL, NULL, NULL, 'RaptorLake', 'F', 'desktop'),
('Intel Core i3-14100', 139, 4, 3.5, 4.7, 60, 4, NULL, NULL, NULL, 'RaptorLake', '', 'desktop');

INSERT INTO cpu
(
    name,
    price_usd,
    core_count,
    core_clock,
    boost_clock,
    tdp,
    socket_id,
    cpu_compute_score,
    cpu_multicore_score,
    cpu_efficiency_score,
    arch,
    suffix,
    segment
)
VALUES
('Intel Core i3-13100T', 189, 4, 2.5, 4.2, 35, 4, NULL, NULL, NULL, 'RaptorLake', 'T', 'mini'),
('Intel Core i5-13500T', 289, 14, 1.6, 4.6, 35, 4, NULL, NULL, NULL, 'RaptorLake', 'T', 'mini'),
('Intel Core i7-13700T', 349, 16, 1.4, 4.9, 35, 4, NULL, NULL, NULL, 'RaptorLake', 'T', 'mini'),
('Intel Core i3-13100', 149, 4, 3.4, 4.5, 60, 4, NULL, NULL, NULL, 'RaptorLake', '', 'mini'),
('Intel Core i5-13400', 229, 10, 2.5, 4.6, 65, 4, NULL, NULL, NULL, 'RaptorLake', '', 'mini'),
('Intel Core i7-1260P', 329, 12, 2.1, 4.7, 28, 4, NULL, NULL, NULL, 'AlderLake', 'P', 'mini'),
('Intel Core i5-1240P', 269, 12, 1.7, 4.4, 28, 4, NULL, NULL, NULL, 'AlderLake', 'P', 'mini'),
('Intel Core i7-1165G7', 299, 4, 2.8, 4.7, 15, 4, NULL, NULL, NULL, 'TigerLake', 'U', 'mini'),
('AMD Ryzen 5 5600G', 199, 6, 3.9, 4.4, 65, 1, NULL, NULL, NULL, 'Zen3', 'G', 'mini'),
('AMD Ryzen 7 5700G', 299, 8, 3.8, 4.6, 65, 1, NULL, NULL, NULL, 'Zen3', 'G', 'mini'),
('AMD Ryzen 7 7735U', 349, 8, 2.7, 4.75, 28, 2, NULL, NULL, NULL, 'Zen3+', 'U', 'mini'),
('AMD Ryzen 5 7535U', 289, 6, 2.9, 4.55, 28, 2, NULL, NULL, NULL, 'Zen3+', 'U', 'mini'),
('AMD Ryzen 7 6800U', 329, 8, 2.7, 4.7, 15, 2, NULL, NULL, NULL, 'Zen3+', 'U', 'mini');
GO

INSERT INTO cpu
VALUES
('AMD Ryzen 9 7950X', 599, 16, 4.5, 5.7, 170, 2, NULL, NULL, NULL, 'Zen4', 'X', 'desktop'),
('AMD Ryzen 9 7900X', 449, 12, 4.7, 5.6, 170, 2, NULL, NULL, NULL, 'Zen4', 'X', 'desktop'),
('AMD Ryzen 7 7800X3D', 449, 8, 4.2, 5.0, 120, 2, NULL, NULL, NULL, 'Zen4', 'X3D', 'desktop'),
('AMD Ryzen 5 7600X', 249, 6, 4.7, 5.3, 105, 2, NULL, NULL, NULL, 'Zen4', 'X', 'desktop');

INSERT INTO cpu
VALUES
('AMD Threadripper Pro 7995WX', 9999, 96, 2.5, 5.1, 350, 7, NULL, NULL, NULL, 'Zen4', 'WX', 'workstation'),
('AMD Threadripper 7980X', 4999, 64, 3.2, 5.1, 350, 7, NULL, NULL, NULL, 'Zen4', 'X', 'workstation'),
('AMD EPYC 9654', 11800, 96, 2.4, 3.7, 360, 6, NULL, NULL, NULL, 'Zen4', '', 'server');

INSERT INTO cpu
VALUES
('Intel Xeon W-3375', 4499, 38, 2.5, 4.0, 270, 8, NULL, NULL, NULL, 'IceLake', 'W', 'workstation'),
('Intel Xeon Gold 6348', 3799, 28, 2.6, 3.5, 205, 8, NULL, NULL, NULL, 'IceLake', 'Gold', 'server'),
('Intel Xeon E-2388G', 829, 8, 3.2, 5.1, 95, 3, NULL, NULL, NULL, 'RocketLake', 'G', 'server');

INSERT INTO cpu
VALUES
('Intel Core i9-13980HX', 699, 24, 2.2, 5.6, 55, 4, NULL, NULL, NULL, 'RaptorLake', 'HX', 'laptop'),
('Intel Core i9-13900H', 599, 14, 2.6, 5.4, 45, 4, NULL, NULL, NULL, 'RaptorLake', 'H', 'laptop'),
('Intel Core i7-13700H', 449, 14, 2.4, 5.0, 45, 4, NULL, NULL, NULL, 'RaptorLake', 'H', 'laptop'),
('Intel Core i7-1360P', 389, 12, 2.2, 5.0, 28, 4, NULL, NULL, NULL, 'RaptorLake', 'P', 'laptop'),
('Intel Core i7-1355U', 329, 10, 1.7, 5.0, 15, 4, NULL, NULL, NULL, 'RaptorLake', 'U', 'laptop'),
('Intel Core i5-13500H', 299, 12, 2.6, 4.7, 45, 4, NULL, NULL, NULL, 'RaptorLake', 'H', 'laptop'),
('Intel Core i5-1340P', 249, 12, 1.9, 4.6, 28, 4, NULL, NULL, NULL, 'RaptorLake', 'P', 'laptop'),
('Intel Core i5-1335U', 219, 10, 1.8, 4.6, 15, 4, NULL, NULL, NULL, 'RaptorLake', 'U', 'laptop'),
('Intel Core i3-1315U', 179, 6, 1.2, 4.5, 15, 4, NULL, NULL, NULL, 'RaptorLake', 'U', 'laptop'),
('Intel Core i3-13100H', 199, 8, 2.4, 4.5, 45, 4, NULL, NULL, NULL, 'RaptorLake', 'H', 'laptop');

INSERT INTO cpu
VALUES
('AMD Ryzen 9 7945HX', 699, 16, 2.5, 5.4, 55, 4, NULL, NULL, NULL, 'Zen4', 'HX', 'laptop'),
('AMD Ryzen 9 7940HS', 599, 8, 4.0, 5.2, 45, 4, NULL, NULL, NULL, 'Zen4', 'HS', 'laptop'),
('AMD Ryzen 7 7840HS', 449, 8, 3.8, 5.1, 45, 4, NULL, NULL, NULL, 'Zen4', 'HS', 'laptop'),
('AMD Ryzen 7 7735H', 399, 8, 3.2, 4.75, 45, 4, NULL, NULL, NULL, 'Zen3+', 'H', 'laptop'),
('AMD Ryzen 7 7730U', 329, 8, 2.0, 4.5, 15, 4, NULL, NULL, NULL, 'Zen3', 'U', 'laptop'),
('AMD Ryzen 5 7640HS', 299, 6, 4.3, 5.0, 45, 4, NULL, NULL, NULL, 'Zen4', 'HS', 'laptop'),
('AMD Ryzen 5 7535U', 249, 6, 2.9, 4.55, 15, 4, NULL, NULL, NULL, 'Zen3+', 'U', 'laptop'),
('AMD Ryzen 5 5500U', 219, 6, 2.1, 4.0, 15, 4, NULL, NULL, NULL, 'Zen2', 'U', 'laptop'),
('AMD Ryzen 3 7330U', 179, 4, 2.3, 4.3, 15, 4, NULL, NULL, NULL, 'Zen3', 'U', 'laptop'),
('AMD Ryzen 3 5300U', 159, 4, 2.6, 3.8, 15, 4, NULL, NULL, NULL, 'Zen2', 'U', 'laptop');

GO
DECLARE @i INT = 1;
DECLARE @tier INT;
DECLARE @suffix VARCHAR(10);
DECLARE @segment VARCHAR(20);
DECLARE @cores INT;
DECLARE @base FLOAT;
DECLARE @boost FLOAT;
DECLARE @tdp INT;
DECLARE @price FLOAT;

WHILE @i <= 250
BEGIN
    SET @tier = (@i % 3);

    IF @tier = 0
        BEGIN
            SET @cores = 10;
            SET @base = 2.5;
            SET @boost = 4.8;
            SET @price = 230;
        END
    ELSE IF @tier = 1
        BEGIN
            SET @cores = 16;
            SET @base = 3.0;
            SET @boost = 5.4;
            SET @price = 380;
        END
    ELSE
        BEGIN
            SET @cores = 24;
            SET @base = 3.2;
            SET @boost = 5.8;
            SET @price = 580;
        END

    IF @i % 10 = 0
        BEGIN
            SET @suffix = 'T';
            SET @segment = 'mini';
            SET @tdp = 35;
            SET @price = @price * 0.9;

        IF @segment = 'mini'
            BEGIN
                SET @cores = CASE
                    WHEN @cores >= 16 THEN 8
                    WHEN @cores > 12 THEN 12
                    ELSE @cores
                END;
                SET @tdp = CASE
                    WHEN @tdp > 45 THEN 35
                    ELSE @tdp
                END;
                SET @suffix = 'T';
            END
        END
    ELSE IF @i % 5 = 0
        BEGIN
            SET @suffix = 'K';
            SET @segment = 'desktop';
            SET @tdp = 125;
        END
    ELSE
        BEGIN
            SET @suffix = '';
            SET @segment = 'desktop';
            SET @tdp = 65;
        END

        INSERT INTO cpu
        (name, price_usd, core_count, core_clock, boost_clock, tdp, socket_id,
         cpu_compute_score, cpu_multicore_score, cpu_efficiency_score,
         arch, suffix, segment)
        VALUES
        (
            CONCAT('Intel Core ',
                   CASE WHEN @tier=0 THEN 'i5'
                        WHEN @tier=1 THEN 'i7'
                        ELSE 'i9' END,
                   '-14', FORMAT(@i,'00'), @suffix),
            @price + (@i * 3),
            @cores,
            @base,
            @boost,
            @tdp,
            4,
            NULL,NULL,NULL,
            'RaptorLake',
            @suffix,
            @segment
        );

        SET @i += 1;
    END
GO

GO
DECLARE @i INT = 1;
DECLARE @cores INT;
DECLARE @suffix VARCHAR(10);
DECLARE @segment VARCHAR(20);
DECLARE @tdp INT;
DECLARE @price FLOAT;

WHILE @i <= 200
BEGIN
    IF @i % 4 = 0
    BEGIN
        SET @cores = 16;
        SET @suffix = 'X';
        SET @price = 580;
    END
    ELSE IF @i % 3 = 0
    BEGIN
        SET @cores = 12;
        SET @suffix = '';
        SET @price = 420;
    END
    ELSE
    BEGIN
        SET @cores = 8;
        SET @suffix = 'X';
        SET @price = 320;
    END

    SET @segment = 'desktop';
    SET @tdp = CASE WHEN @suffix='X' THEN 105 ELSE 65 END;

    INSERT INTO cpu
    (name, price_usd, core_count, core_clock, boost_clock, tdp, socket_id,
     cpu_compute_score, cpu_multicore_score, cpu_efficiency_score,
     arch, suffix, segment)
    VALUES
    (
        CONCAT('AMD Ryzen ',
               CASE WHEN @cores=16 THEN '9'
                    WHEN @cores=12 THEN '9'
                    ELSE '7' END,
               ' 7', FORMAT(@i,'00'), @suffix),
        @price + (@i * 4),
        @cores,
        4.2,
        5.4,
        @tdp,
        2,
        NULL,NULL,NULL,
        'Zen4',
        @suffix,
        @segment
    );

    SET @i += 1;
END
GO

INSERT INTO cpu
(name, price_usd, core_count, core_clock, boost_clock, tdp, socket_id,
 cpu_compute_score, cpu_multicore_score, cpu_efficiency_score,
 arch, suffix, segment)
SELECT
    CONCAT('AMD Threadripper ', 7900 + n, 'WX'),
    3500 + n*120,
    32 + (n%4)*16,
    3.0,
    5.0,
    280,
    7,
    NULL,NULL,NULL,
    'Zen4',
    'WX',
    'workstation'
FROM (SELECT TOP 40 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) n FROM sys.objects) t;

INSERT INTO cpu
SELECT
    CONCAT('Intel Xeon Gold ', 6300 + n),
    4200 + n*150,
    24 + (n%4)*8,
    2.6,
    3.8,
    240,
    8,
    NULL,NULL,NULL,
    'IceLake',
    'Gold',
    'server'
FROM (SELECT TOP 50 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) n FROM sys.objects) t;
GO

UPDATE cpu
SET
    cpu_compute_score = core_clock * boost_clock * 200,
    cpu_multicore_score = core_count * boost_clock * 260,
    cpu_efficiency_score =
        (core_count * boost_clock * 260) / NULLIF(tdp,0);
GO

ALTER TABLE cpu ADD
    intended_form_factor VARCHAR(20),
    power_class VARCHAR(10),
    is_low_power BIT,    
    is_overclockable BIT, 
    is_server_class BIT, 
    is_x3d BIT
GO

UPDATE cpu
SET intended_form_factor =
    CASE
        WHEN segment IN ('mini') THEN 'Mini-PC'
        WHEN segment = 'desktop' THEN 'Tower'
        WHEN segment = 'workstation' THEN 'Workstation'
        WHEN segment = 'server' THEN 'Server'
        WHEN segment = 'laptop' THEN 'Laptop'
    END,
    power_class =
    CASE
        WHEN tdp <= 45 THEN 'Low'
        WHEN tdp <= 105 THEN 'Mid'
        ELSE 'High'
    END;

UPDATE cpu
SET
    is_low_power =
        CASE WHEN suffix IN ('T','U') THEN 1 ELSE 0 END,

    is_overclockable =
        CASE WHEN suffix IN ('K','KF','X') THEN 1 ELSE 0 END,

    is_x3d =
        CASE WHEN name LIKE '%3D%' THEN 1 ELSE 0 END,

    is_server_class =
        CASE
            WHEN name LIKE '%Xeon%'
              OR name LIKE '%EPYC%'
              OR name LIKE '%Threadripper%'
            THEN 1 ELSE 0
        END;
GO
ALTER TABLE cpu ADD
    nlp_is_xeon BIT,
    nlp_is_epyc BIT,
    nlp_is_threadripper BIT,
    nlp_is_core_i BIT,
    nlp_is_ryzen BIT;
GO

UPDATE cpu
SET
    nlp_is_xeon = CASE WHEN name LIKE '%Xeon%' THEN 1 ELSE 0 END,
    nlp_is_epyc = CASE WHEN name LIKE '%EPYC%' THEN 1 ELSE 0 END,
    nlp_is_threadripper = CASE WHEN name LIKE '%Threadripper%' THEN 1 ELSE 0 END,
    nlp_is_core_i = CASE WHEN name LIKE '%Core i%' THEN 1 ELSE 0 END,
    nlp_is_ryzen = CASE WHEN name LIKE '%Ryzen%' THEN 1 ELSE 0 END;
GO

UPDATE cpu
SET
    core_count = CASE
        WHEN core_count > 8 THEN 8
        WHEN core_count < 4 THEN 4
        ELSE core_count
    END,
    tdp = CASE
        WHEN tdp < 15 THEN 15
        WHEN tdp > 45 THEN 45
        ELSE tdp
    END,
    suffix = CASE
        WHEN suffix IN ('T','U') THEN suffix
        ELSE 'T'
    END
WHERE segment = 'mini';
GO

UPDATE cpu
SET segment = 'mini'
WHERE tdp BETWEEN 15 AND 45
  AND suffix IN ('T','U');

-- Memory -- 
INSERT INTO memory
SELECT
    CONCAT('RAM-', n),
    CASE 
        WHEN cap = 4   THEN 25
        WHEN cap = 8   THEN 40
        WHEN cap = 12  THEN 55
        WHEN cap = 16  THEN 75
        WHEN cap = 24  THEN 110
        WHEN cap = 32  THEN 140
        WHEN cap = 48  THEN 200
        WHEN cap = 64  THEN 260
        ELSE 480
    END + n * 0.5,
    CASE 
        WHEN n % 3 = 0 THEN 3200
        WHEN n % 3 = 1 THEN 4800
        ELSE 6400
    END,
    CASE 
        WHEN cap >= 32 THEN 2
        ELSE 1
    END,
    cap,
    CASE WHEN cap >= 64 THEN 1 ELSE 0 END,
    CASE WHEN cap >= 64 THEN 1 ELSE 0 END
FROM (
    SELECT
        n,
        CASE
            WHEN n % 9 = 0 THEN 4
            WHEN n % 9 = 1 THEN 8
            WHEN n % 9 = 2 THEN 12
            WHEN n % 9 = 3 THEN 16
            WHEN n % 9 = 4 THEN 24
            WHEN n % 9 = 5 THEN 32
            WHEN n % 9 = 6 THEN 48
            WHEN n % 9 = 7 THEN 64
            ELSE 128
        END AS cap
    FROM #numbers
    WHERE n <= 500
) x;
GO

UPDATE memory
SET
    ram_capacity_score = capacity_gb,
    ram_bandwidth_score = speed_mts * module_count;
GO

-- GPU --
INSERT INTO videocard
SELECT
    CONCAT('GPU-',n),
    120 + n*6,
    75 + (n%8)*50,
    NULL,NULL
FROM #numbers WHERE n<=400;

UPDATE videocard
SET
    gpu_compute_score =
        (power_draw_w * (25 + (gpu_id % 10) * 5)),

    gpu_power_efficiency =
        (power_draw_w * (25 + (gpu_id % 10) * 5))
        / NULLIF(power_draw_w,0);
GO

-- Motherboard --
INSERT INTO motherboard
(name, price_usd, performance_score, socket_id, form_factor_id)
SELECT
    CONCAT('MB-',n),
    100+n,
    500+(n%6)*300,
    (n%4)+1,
    (n%5)+1
FROM #numbers WHERE n<=200;

INSERT INTO motherboard (
    name,
    price_usd,
    performance_score,
    socket_id,
    form_factor_id,
    chipset_id
)
SELECT
    CONCAT('Laptop Logic Board ', s.name, ' v', n.n),
    0,
    800 + n.n * 5,
    s.socket_id,
    mff.form_factor_id,
    c.chipset_id
FROM socket s
CROSS JOIN (VALUES (1),(2),(3),(4),(5),(6),(7),(8)) n(n)
JOIN motherboard_form_factor mff
    ON mff.name = 'Laptop'
JOIN chipset c
    ON c.name IN ('Intel Mobile SoC','AMD Mobile SoC')
WHERE s.name IN ('FP7','BGA1744');

DECLARE @desktop_sockets TABLE(socket_id INT);
INSERT INTO @desktop_sockets VALUES (1),(2),(3),(4);

INSERT INTO motherboard (name, price_usd, performance_score, socket_id, form_factor_id)
SELECT 
    CONCAT('MB-', s.socket_id, '-', ff.name),
    100 + ROW_NUMBER() OVER(ORDER BY (SELECT NULL)),
    500 + ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) * 10,
    s.socket_id,
    ff.form_factor_id
FROM @desktop_sockets s
CROSS JOIN (SELECT form_factor_id, name FROM motherboard_form_factor WHERE name IN ('ATX','Micro-ATX')) ff;

INSERT INTO motherboard
(name, price_usd, performance_score, socket_id, form_factor_id)
VALUES
-- Threadripper / TR Pro
('WRX80 Board 1', 850, 2200, 7, 1), -- E-ATX
('WRX80 Board 2', 900, 2400, 7, 6), -- XL-ATX

-- Xeon W / LGA2066
('Xeon W Board 1', 700, 2000, 8, 1),
('Xeon W Board 2', 780, 2100, 8, 6);

INSERT INTO motherboard (
    name,
    price_usd,
    performance_score,
    socket_id,
    form_factor_id,
    chipset_id
)
VALUES
(
    'Intel Laptop Logic Board (BGA1744)',
    0,              
    800,            
    12,             
    8,              
    97
);

INSERT INTO motherboard (
    name,
    price_usd,
    performance_score,
    socket_id,
    form_factor_id,
    chipset_id
)
VALUES
(
    'AMD Laptop Logic Board (FP7)',
    0,              
    800,           
    12,             
    8,              
    98
);

-- Internal Storage --
INSERT INTO internal_storage
(price_usd, performance_score, model_name, amount_gb)
SELECT 60+n*17, 800+(n%6)*400, CONCAT('Disk-', n), 1024+(n*1024)
FROM #numbers WHERE n<=20;

-- PSU --
INSERT INTO psu
(price_usd, wattage, model_name)
SELECT 80+n*2, 500+(n%5)*250, CONCAT('PSU-',  500+(n%5)*250, 'W - ', n)
FROM #numbers WHERE n<=150;

-- PC Case --
INSERT INTO pc_case (price_usd, case_type) VALUES

-- Tower --
(69,  'Tower'),
(75,  'Tower'),
(82,  'Tower'),
(89,  'Tower'),
(95,  'Tower'),
(105, 'Tower'),
(115, 'Tower'),
(125, 'Tower'),
(135, 'Tower'),
(145, 'Tower'),

-- Workstation --
(165, 'Large Tower'),
(185, 'Large Tower'),
(205, 'Large Tower'),
(225, 'Large Tower'),
(245, 'Large Tower'),
(275, 'Large Tower'),

-- Rackmount --
(299, 'Rackmount'),
(349, 'Rackmount'),
(399, 'Rackmount'),
(449, 'Rackmount'),

-- Laptop Chassis --
(450, 'Laptop-Chassis');

-- Mini-PC --
INSERT INTO pc_case (price_usd, case_type)
VALUES
(45, 'Mini-PC'),
(55, 'Mini-PC'),
(65, 'Mini-PC');

-- Wireless Network Card -- 
INSERT INTO wireless_network_card
(price_usd)
SELECT 20+n
FROM #numbers WHERE n<=150;

-- Insert Mini-PC suitable PSUs
INSERT INTO psu (price_usd, wattage, model_name)
VALUES (50, 35, 'PSU-MINI-A'), (60, 45, 'PSU-MINI-B'), (70, 65, 'PSU-MINI-C');

-- Insert Mini-PC suitable cases
INSERT INTO pc_case(price_usd)
VALUES (40), (50), (60), (70), (80);
GO

-- #7 ~ Populate Computer Builds --

-- Laptop
DECLARE @counter_laptop INT;
SET @counter_laptop = 1;

WHILE @counter_laptop < 100
BEGIN
WITH laptop_cpu AS (
    SELECT id, price_usd, cpu_compute_score,
           ROW_NUMBER() OVER (ORDER BY id) AS rn
    FROM cpu
    WHERE segment = 'laptop'
),
laptop_ram AS (
    SELECT ram_id, capacity_gb, price_usd,
           ROW_NUMBER() OVER (ORDER BY ram_id) AS rn
    FROM memory
    WHERE capacity_gb IN (8,16,32)
),
laptop_mobo AS (
    SELECT id, price_usd
    FROM motherboard
    WHERE name LIKE '%Laptop Logic Board%'
),
laptop_case AS (
    SELECT id FROM pc_case WHERE case_type = 'Laptop-Chassis'
),
  weak_gpu AS (
    SELECT gpu_id, price_usd, gpu_compute_score
    FROM videocard
    WHERE gpu_compute_score <= 7500
)
INSERT INTO computer_build (
    cpu_id, motherboard_id, ram_id, storage_id, gpu_id, case_id,
    total_price_usd, total_performance_score,
    has_gpu, form_factor_label, is_portable
)
SELECT TOP (2)
    cpu.id,
    mobo.id,
    ram.ram_id,
    storage.id,
    CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN gpu.gpu_id ELSE NULL END,
    (SELECT id FROM pc_case WHERE case_type = 'Laptop-Chassis'),
    cpu.price_usd + mobo.price_usd + ram.price_usd
        + ISNULL(gpu.price_usd, 0),
    cpu.cpu_compute_score
        + ISNULL(gpu.gpu_compute_score, 0),
    CASE WHEN gpu.gpu_id IS NULL THEN 0 ELSE 1 END,
    'Laptop',
    1
FROM laptop_cpu cpu
CROSS JOIN laptop_mobo mobo
JOIN laptop_ram ram
  ON ram.rn = ((cpu.rn - 1) % (SELECT COUNT(*) FROM laptop_ram)) + 1
OUTER APPLY (
 SELECT TOP 1 *
    FROM (
        SELECT *,
               ABS(CHECKSUM(NEWID())) AS rnd
        FROM videocard
        WHERE gpu_compute_score <= 7500
    ) x
    ORDER BY rnd
) gpu
OUTER APPLY (
    SELECT TOP 1 id, price_usd
    FROM internal_storage
    ORDER BY NEWID()
) storage;
  SET @counter_laptop = @counter_laptop + 1;
END;

-- Gaming Laptop --
SELECT gpu_id, COUNT(*) usage_count
FROM computer_build
WHERE form_factor_label = 'Laptop'
GROUP BY gpu_id
ORDER BY usage_count DESC;

  DECLARE @counter_laptop_gaming INT;
SET @counter_laptop_gaming = 1;

WHILE @counter_laptop_gaming < 100
BEGIN

  WITH laptop_cpu AS (
    SELECT id, price_usd, cpu_compute_score,
           ROW_NUMBER() OVER (ORDER BY id) AS rn
    FROM cpu
    WHERE segment = 'laptop'
),
laptop_ram AS (
    SELECT ram_id, capacity_gb, price_usd,
           ROW_NUMBER() OVER (ORDER BY ram_id) AS rn
    FROM memory
    WHERE capacity_gb >= 16
),
laptop_mobo AS (
    SELECT id, price_usd
    FROM motherboard
    WHERE name LIKE '%Laptop Logic Board%'
),
laptop_case AS (
    SELECT id FROM pc_case WHERE case_type = 'Laptop-Chassis'
),
  gaming_gpu AS (
    SELECT gpu_id, price_usd, gpu_compute_score
    FROM videocard
    WHERE gpu_compute_score >= 7500
)
INSERT INTO computer_build (
    cpu_id, motherboard_id, ram_id, storage_id, gpu_id, case_id,
    total_price_usd, total_performance_score,
    has_gpu, form_factor_label, is_portable
)
SELECT TOP (2)
    cpu.id,
    mobo.id,
    ram.ram_id,
    storage.id,
    gpu.gpu_id,
    (SELECT id FROM pc_case WHERE case_type = 'Laptop-Chassis'),
    cpu.price_usd + mobo.price_usd + ram.price_usd + gpu.price_usd,
    cpu.cpu_compute_score + gpu.gpu_compute_score,
    1,
    'Gaming Laptop',
    1
FROM laptop_cpu cpu
CROSS JOIN laptop_mobo mobo
JOIN laptop_ram ram
  ON ram.rn = ((cpu.rn - 1) % (SELECT COUNT(*) FROM laptop_ram)) + 1
OUTER APPLY (
 SELECT TOP 1 *
    FROM (
        SELECT *,
               ABS(CHECKSUM(NEWID())) AS rnd
        FROM videocard
        WHERE gpu_compute_score > 7500
    ) x
    ORDER BY rnd
) gpu

OUTER APPLY (
    SELECT TOP 1 id, price_usd
    FROM internal_storage
    ORDER BY NEWID()
) storage;

SET @counter_laptop_gaming = @counter_laptop_gaming + 1;
END;

-- Mini-PC --
WITH mini_cpu AS (
    SELECT id, core_count, socket_id, price_usd, cpu_compute_score,
           ROW_NUMBER() OVER (ORDER BY id) AS rn
    FROM cpu
    WHERE segment = 'mini'
),
mini_ram AS (
    SELECT ram_id, capacity_gb, price_usd,
           ROW_NUMBER() OVER (ORDER BY ram_id) AS rn
    FROM memory
    WHERE capacity_gb IN (8, 16, 32)
),
mini_mobo AS (
    SELECT mobo.id, mobo.socket_id,
           ROW_NUMBER() OVER (PARTITION BY mobo.socket_id ORDER BY mobo.id) AS rn
    FROM motherboard mobo
    JOIN motherboard_form_factor mff
      ON mobo.form_factor_id = mff.form_factor_id
    WHERE mff.name IN ('Mini-ITX','Mini-DTX')
),
mini_case AS (
    SELECT TOP 1 id FROM pc_case WHERE case_type = 'Mini-PC'
)
INSERT INTO computer_build (
    cpu_id,
    motherboard_id,
    ram_id,
    storage_id,
    case_id,
    total_price_usd,
    total_performance_score,
    has_gpu,
    form_factor_label,
    is_portable
)
SELECT TOP (100)
    cpu.id,
    mobo.id,
    ram.ram_id,
    storage.id,
    (SELECT id FROM mini_case),
    cpu.price_usd + ram.price_usd,
    cpu.cpu_compute_score,
    0,
    'Mini-PC',
    0
FROM mini_cpu cpu
JOIN mini_mobo mobo ON mobo.socket_id = cpu.socket_id
JOIN mini_ram ram
  ON ram.rn = ((cpu.rn - 1) % (SELECT COUNT(*) FROM mini_ram)) + 1

OUTER APPLY (
    SELECT TOP 1 id, price_usd
    FROM internal_storage
    ORDER BY NEWID()
) storage

ORDER BY cpu.rn, mobo.rn;

-- Tower --
DECLARE @counter_tower INT;
SET @counter_tower = 1;

WHILE @counter_tower <= 100
BEGIN
WITH tower_cpu AS (
    SELECT id, price_usd, tdp, cpu_compute_score, 
           ROW_NUMBER() OVER (ORDER BY id) AS rn
    FROM cpu
    WHERE segment = 'desktop'
),
tower_ram AS (
    SELECT ram_id, capacity_gb, price_usd,
           ROW_NUMBER() OVER (ORDER BY ram_id) AS rn
    FROM memory
    WHERE capacity_gb IN (8,16,32)
),
tower_mobo AS (
    SELECT id, price_usd
    FROM motherboard
    WHERE name NOT LIKE '%Laptop%'
),
tower_case AS (
    SELECT id
    FROM pc_case
    WHERE case_type = 'Tower'
),
weak_gpu AS (
    SELECT gpu_id, price_usd, gpu_compute_score
    FROM videocard
    WHERE gpu_compute_score <= 10000
)
INSERT INTO computer_build (
    cpu_id, motherboard_id, ram_id,
    gpu_id, storage_id, psu_id, case_id,
    total_price_usd, total_performance_score,
    has_gpu, form_factor_label, is_portable
)
SELECT TOP (2)
    cpu.id,
    mobo.id,
    ram.ram_id,
    gpu.gpu_id,
    storage.id,
    psu.psu_id,
    (SELECT TOP 1 id FROM pc_case WHERE case_type = 'Tower' ORDER BY NEWID()),
    cpu.price_usd
    + mobo.price_usd
    + ram.price_usd
    + ISNULL(gpu.price_usd, 0)
    + storage.price_usd
    + psu.price_usd,
    cpu.cpu_compute_score
    + ISNULL(gpu.gpu_compute_score, 0),
    CASE WHEN gpu.gpu_id IS NULL THEN 0 ELSE 1 END,
    'Tower',
    0
FROM tower_cpu cpu
CROSS JOIN tower_mobo mobo
JOIN tower_ram ram
  ON ram.rn = ((cpu.rn - 1) % (SELECT COUNT(*) FROM tower_ram)) + 1
OUTER APPLY (
    SELECT TOP 1 *
    FROM videocard
    WHERE gpu_compute_score <= 10000
    ORDER BY NEWID()
) gpu
OUTER APPLY (
    SELECT TOP 1 id, price_usd
    FROM internal_storage
    ORDER BY NEWID()
) storage
OUTER APPLY (
    SELECT TOP 1 psu_id, price_usd
    FROM psu
    WHERE wattage >= ISNULL(gpu.power_draw_w,0) + cpu.tdp + 150
    ORDER BY NEWID()
) psu;

SET @counter_tower = @counter_tower + 1;
END;

-- Gaming Tower --
DECLARE @counter_gaming_tower INT;
SET @counter_gaming_tower = 1;

WHILE @counter_gaming_tower <= 100
BEGIN
WITH tower_cpu AS (
    SELECT id, price_usd, tdp, cpu_compute_score,
           ROW_NUMBER() OVER (ORDER BY id) AS rn
    FROM cpu
    WHERE segment = 'desktop'
),
tower_ram AS (
    SELECT ram_id, capacity_gb, price_usd,
           ROW_NUMBER() OVER (ORDER BY ram_id) AS rn
    FROM memory
    WHERE capacity_gb >= 16
),
tower_mobo AS (
    SELECT id, price_usd, form_factor_id
    FROM motherboard
    WHERE name NOT LIKE '%Laptop%' AND (form_factor_id = 2)
),
tower_case AS (
    SELECT id
    FROM pc_case
    WHERE case_type = 'Tower'
),
gaming_gpu AS (
    SELECT gpu_id, price_usd, gpu_compute_score
    FROM videocard
    WHERE gpu_compute_score > 10000
)
INSERT INTO computer_build (
    cpu_id, motherboard_id, ram_id,
    gpu_id, storage_id, psu_id, case_id,
    total_price_usd, total_performance_score,
    has_gpu, form_factor_label, is_portable
)
SELECT TOP (2)
    cpu.id,
    mobo.id,
    ram.ram_id,
    gpu.gpu_id,
    storage.id,
    psu.psu_id,
    (SELECT TOP 1 id FROM pc_case WHERE case_type = 'Tower'),
    cpu.price_usd + mobo.price_usd + ram.price_usd + gpu.price_usd,
    cpu.cpu_compute_score + gpu.gpu_compute_score,
    1,
    'Gaming Tower',
    0
FROM tower_cpu cpu
CROSS JOIN tower_mobo mobo
JOIN tower_ram ram
  ON ram.rn = ((cpu.rn - 1) % (SELECT COUNT(*) FROM tower_ram)) + 1
OUTER APPLY (
    SELECT TOP 1 *
    FROM (
        SELECT *, ABS(CHECKSUM(NEWID())) AS rnd
        FROM videocard
        WHERE gpu_compute_score >= 10000
    ) x
    ORDER BY rnd
) gpu
OUTER APPLY (
    SELECT TOP 1 id, price_usd
    FROM internal_storage
    ORDER BY NEWID()
) storage
OUTER APPLY (
    SELECT TOP 1 psu_id, price_usd
    FROM psu
    WHERE wattage >= ISNULL(gpu.power_draw_w,0) + cpu.tdp + 150
    ORDER BY NEWID()
) psu;

WITH tower_cpu AS (
    SELECT id, price_usd, tdp, cpu_compute_score,
           ROW_NUMBER() OVER (ORDER BY id) AS rn
    FROM cpu
    WHERE segment = 'desktop'
),
tower_ram AS (
    SELECT ram_id, capacity_gb, price_usd,
           ROW_NUMBER() OVER (ORDER BY ram_id) AS rn
    FROM memory
    WHERE capacity_gb >= 16
),
tower_mobo AS (
    SELECT id, price_usd, form_factor_id
    FROM motherboard
    WHERE name NOT LIKE '%Laptop%' AND (form_factor_id = 3)
),
tower_case AS (
    SELECT id
    FROM pc_case
    WHERE case_type = 'Tower'
),
gaming_gpu AS (
    SELECT gpu_id, price_usd, gpu_compute_score
    FROM videocard
    WHERE gpu_compute_score > 10000
)
INSERT INTO computer_build (
    cpu_id, motherboard_id, ram_id,
    gpu_id, storage_id, psu_id, case_id,
    total_price_usd, total_performance_score,
    has_gpu, form_factor_label, is_portable
)
SELECT TOP (2)
    cpu.id,
    mobo.id,
    ram.ram_id,
    gpu.gpu_id,
    storage.id,
    psu.psu_id,
    (SELECT TOP 1 id FROM pc_case WHERE case_type = 'Tower'),
    cpu.price_usd + mobo.price_usd + ram.price_usd + gpu.price_usd,
    cpu.cpu_compute_score + gpu.gpu_compute_score,
    1,
    'Gaming Tower',
    0
FROM tower_cpu cpu
CROSS JOIN tower_mobo mobo
JOIN tower_ram ram
  ON ram.rn = ((cpu.rn - 1) % (SELECT COUNT(*) FROM tower_ram)) + 1
OUTER APPLY (
    SELECT TOP 1 *
    FROM (
        SELECT *, ABS(CHECKSUM(NEWID())) AS rnd
        FROM videocard
        WHERE gpu_compute_score >= 10000
    ) x
    ORDER BY rnd
) gpu
OUTER APPLY (
    SELECT TOP 1 id, price_usd
    FROM internal_storage
    ORDER BY NEWID()
) storage
OUTER APPLY (
    SELECT TOP 1 psu_id, price_usd
    FROM psu
    WHERE wattage >= ISNULL(gpu.power_draw_w,0) + cpu.tdp + 150
    ORDER BY NEWID()
) psu;

SET @counter_gaming_tower = @counter_gaming_tower + 1;
END;

-- Workstation
INSERT INTO computer_build (
    cpu_id, motherboard_id, ram_id, storage_id, psu_id, case_id,
    total_price_usd, total_performance_score,
    has_gpu, form_factor_label, is_portable
)
SELECT TOP (200)
    cpu.id,
    mobo.id,
    ram.ram_id,
    storage.id,
    psu.psu_id,
    (SELECT TOP 1 id FROM pc_case WHERE case_type = 'Large Tower'),
    cpu.price_usd + mobo.price_usd + ram.price_usd,
    cpu.cpu_compute_score,
    0,
    'Workstation',
    0
FROM cpu cpu
JOIN motherboard mobo ON mobo.socket_id = cpu.socket_id
JOIN memory ram ON ram.capacity_gb >= 32
OUTER APPLY (
    SELECT TOP 1 id
    FROM internal_storage
    WHERE amount_gb > 10000
    ORDER BY NEWID()
) storage
OUTER APPLY (
    SELECT TOP 1 psu_id, price_usd
    FROM psu
    WHERE wattage >= cpu.tdp + 150
    ORDER BY NEWID()
) psu
WHERE cpu.segment = 'workstation';

-- Server
INSERT INTO computer_build (
    cpu_id, motherboard_id, ram_id,  storage_id, psu_id, case_id,
    total_price_usd, total_performance_score,
    has_gpu, form_factor_label, is_portable
)
SELECT TOP (200)
    cpu.id,
    mobo.id,
    ram.ram_id,
    storage.id,
    psu.psu_id,
    (SELECT TOP 1 id FROM pc_case WHERE case_type = 'Rackmount'),
    cpu.price_usd + mobo.price_usd + ram.price_usd,
    cpu.cpu_compute_score,
    0,
    'Server',
    0
FROM cpu cpu
JOIN motherboard mobo ON mobo.socket_id = cpu.socket_id
JOIN memory ram ON ram.capacity_gb >= 64
OUTER APPLY (
    SELECT TOP 1 id
    FROM internal_storage
    WHERE amount_gb > 10000
    ORDER BY NEWID()
) storage
OUTER APPLY (
    SELECT TOP 1 psu_id, price_usd
    FROM psu
    WHERE wattage >= cpu.tdp + 150
    ORDER BY NEWID()
) psu
WHERE cpu.segment = 'server';
GO

-- #8 ~ Create VIEW to train the Machine learning Model --

-- Value Prediction Training View for Model Machine Learning --
CREATE OR ALTER VIEW ml_value_prediction_training_view AS
SELECT
    b.build_id,
    b.total_price_usd AS Total_price_usd,
    cpu.cpu_compute_score       AS Cpu_compute_score,
    cpu.cpu_multicore_score     AS Cpu_multicore_score,
    cpu.cpu_efficiency_score    AS Cpu_efficiency_score,
    ISNULL(gpu.gpu_compute_score, 0)     AS Gpu_compute_score,
    ISNULL(gpu.gpu_power_efficiency, 0)  AS Gpu_power_efficiency,
    ram.ram_capacity_score      AS Ram_capacity_score,
    ram.ram_bandwidth_score     AS Ram_bandwidth_score,
    CAST(b.has_gpu AS FLOAT)    AS Has_gpu,
    b.form_factor_label         AS Computer_type
FROM computer_build b
JOIN cpu 
    ON b.cpu_id = cpu.id
JOIN memory ram 
    ON b.ram_id = ram.ram_id
LEFT JOIN videocard gpu 
    ON b.gpu_id = gpu.gpu_id;
GO

-- Data Classification Training View for Model Machine Learning --
CREATE OR ALTER VIEW ml_data_classification_training_view AS
SELECT
    b.build_id,
    mff.name AS motherboard_form_factor,                        
    cs.case_type,                                               
    ISNULL(gpu.gpu_compute_score, 0) AS gpu_compute_score,      
    cpu.intended_form_factor AS cpu_form_factor,                
    b.form_factor_label                                         
FROM computer_build b
JOIN cpu
    ON b.cpu_id = cpu.id
LEFT JOIN motherboard mb
    ON b.motherboard_id = mb.id
LEFT JOIN motherboard_form_factor mff
    ON mb.form_factor_id = mff.form_factor_id
LEFT JOIN pc_case cs
    ON b.case_id = cs.id
LEFT JOIN videocard gpu
    ON b.gpu_id = gpu.gpu_id;
GO