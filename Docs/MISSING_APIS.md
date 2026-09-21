# Missing Backend APIs for Full Admin Panel



The Admin Panel (`HospitalAdminPanel`) consumes **only** existing APIs. Remaining gaps are listed below.



## Currently available admin APIs



| Feature | Endpoint |

|---------|----------|

| Admin login | `POST /api/admin/auth/login` |

| Dashboard counts | `GET /api/admin/dashboard` |

| Message threads + reply | `GET/POST /api/admin/messages/*` |

| Thread conversation history | `GET /api/admin/messages/threads/{id}/messages` |

| Portal user list | `GET /api/admin/users?search=&page=` |

| Appointment list + approve/reject | `GET /api/admin/appointments`, `POST .../approve`, `POST .../reject` |

| Refill approve/reject | `GET/PUT /api/admin/refills/*` |

| Support tickets | `GET/PUT /api/admin/support/tickets` |

| Audit log | `GET /api/admin/audit` (Admin role) |

| Analytics | `GET /api/admin/analytics/users|visits|engagement` |

| Registration report | `GET /api/admin/reports/registrations?from=&to=` |

| Appointment report | `GET /api/admin/reports/appointments?status=&from=&to=` |

| Engagement export | `GET /api/admin/reports/engagement/export` |

| Patient lookup by MR | `GET /api/Patient?MR_NO=` (Staff JWT) |



## Optional future enhancements



- Push notification to patient on appointment approve/reject

- Per-user analytics on user detail page

- Bulk appointment actions


