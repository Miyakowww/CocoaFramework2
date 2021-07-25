# 数据存储

可以使用 DataManager.SaveData 和 DataManager.LoadData 进行数据的存储和读取，也可以在 Module 和 Middleware 中为字段添加 Hosting 特性以自动读取和保存数据。  
此类方式仅限轻量数据的存储，如配置信息、用户数据等。完整消息记录等庞大的数据继续使用此方式可能造成过大的性能消耗，此类数据建议使用数据库存储。