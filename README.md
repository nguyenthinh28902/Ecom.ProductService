//Lệnh tạo 
Multi-Version Concurrency Control (MVCC)
(nơi mà lệnh Đọc không bị chặn bởi lệnh Ghi và ngược lại)
ALTER DATABASE ecom_product_db 
SET ALLOW_SNAPSHOT_ISOLATION ON;
go
ALTER DATABASE ecom_product_db 
SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;
