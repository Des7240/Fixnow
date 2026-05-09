docker run -d ^
  --name fixnow-postgres ^
  -e POSTGRES_DB=fixnow_db ^
  -e POSTGRES_USER=fixnow ^
  -e POSTGRES_PASSWORD=123456 ^
  -p 5432:5432 ^
  postgis/postgis

  docker run -d ^
  --name fixnow-pgadmin ^
  -e PGADMIN_DEFAULT_EMAIL=admin@fixnow.com ^
  -e PGADMIN_DEFAULT_PASSWORD=admin123 ^
  -p 5050:80 ^
  dpage/pgadmin4

  redis
  docker run -d `
  --name fixnow-redis `
  -p 6379:6379 `
  redis


    {
   2       "email": "admin@fixnow.com",
   3       "password": "AdminPassword123",
   4       "fullName": "Administrator",
   5       "role": "ADMIN"
   6     }