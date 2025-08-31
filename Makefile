.PHONY: dev prod clean logs shell

dev:
	docker-compose --env-file .env.development up --build

prod:
	docker-compose --env-file .env.production up --build -d

clean:
	docker-compose down --volumes --remove-orphans
	docker system prune -f

logs:
	docker-compose logs -f

shell-api:
	docker exec -it claimiq-claimiq-api-1 /bin/bash

shell-web:
	docker exec -it claimiq-claimiq-web-1 /bin/sh

health:
	curl -f http://localhost:5234/health && curl -f http://localhost:5001
