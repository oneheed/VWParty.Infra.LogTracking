# References

* 使用 NGINX 當作前端 Reverse Proxy, 如何自動產生 unique id, 放在 request headers 內? 
  * 放 guid 的方式: [Setting a trace id in nginx load balancer](https://stackoverflow.com/questions/17748735/setting-a-trace-id-in-nginx-load-balancer)
  * 放 start time:  [Module ngx_http_headers_module](http://nginx.org/en/docs/http/ngx_http_headers_module.html)