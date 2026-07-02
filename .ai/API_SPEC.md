# Authentication

POST /api/auth/register

POST /api/auth/login

POST /api/auth/refresh-token

POST /api/auth/logout

---

# Tickets

POST /api/tickets

GET /api/tickets/my-tickets

GET /api/tickets/{id}

GET /api/tickets/assigned

PUT /api/tickets/{id}/status

POST /api/tickets/{id}/reply

GET /api/tickets/{id}/replies

---

# Admin

GET /api/admin/tickets

PUT /api/admin/tickets/{id}/assign-agent

GET /api/admin/dashboard

---

# Users

GET /api/users

GET /api/users/{id}

PUT /api/users/{id}

DELETE /api/users/{id}