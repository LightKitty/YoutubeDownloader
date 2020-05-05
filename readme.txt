linux服务器部署运行报错 System.Net.Http.HttpRequestException: The SSL connection could not be establ ...

在发布文件 *runtimeconfig.json 中添加配置 

"runtimeOptions": {
  "configProperties": {
      "System.Net.Http.UseSocketsHttpHandler": false
  }
}
参考 https://www.cnblogs.com/CnKker/p/11423471.html