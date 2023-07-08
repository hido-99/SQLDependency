## Broker Service In SQL Server  -  class SQL Dependency in .NET

Đồng bộ dữ liệu tự động từ 1 table trong cơ sở dữ liệu đến các máy client

> KÍCH HOẠT DỊCH VỤ BROKER TRONG SQL SERVER
```
ALTER DATABASE [TEN_DATABASE] SET ENABLE_BROKER ;
```
> Xem dịch vụ Broker đã hoạt động chưa và Id của nó:
 ```
SELECT name , is_broker_enabled, service_broker_guid
FROM SYS.DATABASES
```
> Khai báo Class SQL Dependency:

`1.	Khởi tạo một SqlDependency kết nối đến Server. `

`2.	Tạo đối tượng SqlConnection kết nối đến Server và đối tượng  SqlCommand chứa lệnh Transact - SQL.`

`3.	Tạo một đối tượng SqlDependency mới , hoặc sử dụng đối tượng đang tồn tại, và kết nó vào đối tượng SqlCommand đã tạo ở bước 2. Điều này sẽ tạo ra một đối tượng SqlNotificationRequest và kết nó vào đối tượng lệnh cần thiết. `

`4.	Khai báo xử lý cho sự kiện OnChange của đối tượng SqlDependency .`

`5.	Thực hiện lệnh Execute của đối tượng SqlCommand . Bởi vì đã lệnh được ràng buộc với đối tượng thông báo , Server nhận ra rằng nó phải tạo ra một thông báo, và các thông tin hàng đợi sẽ trỏ đến hàng đợi dependencies.`

`6.	Ngừng kết nối SqlDependency đến Server.`

>Sử dụng Isolation để giải quyết vấn đề dirty data, phantom row nảy sinh khi có nhiều user cùng thao tác lên dữ liệu cùng 1 thời điểm.
```
Ví dụ: StoredProcedure [dbo].[SP_WRITE]  
GO 
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[SP_WRITE] @ID int, @HO NVARCHAR(50), @TEN NVARCHAR(50), @PHAI NVARCHAR(5),
	@DIACHI NVARCHAR(200), @NGAYSINH DATE, @LUONG FLOAT 
AS
BEGIN
	-- SET ISOLATION LEVEL --
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
	BEGIN TRAN
	
	-- CHECK WHETHER ID HAS ALREADY EXISTED
	DECLARE @CHECKID BIT
	SET @CHECKID = 0

	IF EXISTS(SELECT MANV FROM NHANVIEN WHERE MANV = @ID)
	BEGIN
		SET @CHECKID = 1
	END
	
	-- IF ID HAS ALREADY EXISTED THEN EXECUTE UPDATING --
	-- IF ID HAS NOT ALREADY EXISTED THEN EXECUTE INSERTING --
	IF @CHECKID = 1
	BEGIN
		UPDATE NHANVIEN SET HO=@HO, TEN = @TEN, PHAI = @PHAI, DIACHI = @DIACHI, NGAYSINH = @NGAYSINH,
			LUONG = @LUONG WHERE MANV = @ID
	END
	
	ELSE
	BEGIN
		INSERT INTO NHANVIEN VALUES (@HO, @TEN, @PHAI, @DIACHI, @NGAYSINH, @LUONG)
	END

	-- WAIT -- 
	WAITFOR DELAY '00:00:30'

	COMMIT
END
```
