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
