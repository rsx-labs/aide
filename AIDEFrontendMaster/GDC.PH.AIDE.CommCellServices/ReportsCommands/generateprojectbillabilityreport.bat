@echo off
"C:\Program Files (x86)\kool-aide\kool-aide" generate-report -r project-billability --format excel --output "C:\GeneratedReports\Retail Services Project Billability.xlsx" --params {\"sorts\":[\"EmployeeName\"],\"departments\":[1],\"divisions\":[1],\"fys\":[\"%1\"]}