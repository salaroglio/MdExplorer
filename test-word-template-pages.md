---
title: "Report Tecnico Sistema XYZ"
author: "Mario Rossi"
date: "2024-01-15"
word_section:
  write_toc: true
  template_section:
    template_type: inherits
    inherit_from_template: project
  predefined_pages:
    cover:
      type: covers
      template: project
      enabled: true
      tags:
        project_name: "Sistema XYZ"
        document_type: "Report Tecnico"
        project_code: "XYZ-2024-001"
        client_name: "Cliente ABC"
        version: "1.0"
        start_date: "01/01/2024"
        end_date: "31/12/2024"
        approver_name: "Giuseppe Verdi"
        approver_role: "Project Manager"
        status: "Final"
    disclaimer:
      type: disclaimers
      template: confidential
      enabled: true
      tags:
        company: "Azienda SpA"
        classification: "Confidential"
        expiry_date: "31/12/2025"
        contact_email: "info@azienda.com"
    appendix:
      type: appendices
      template: signatures
      enabled: true
      tags:
        role_1: "Project Manager"
        name_1: "Giuseppe Verdi"
        role_2: "Technical Lead"
        name_2: "Luigi Bianchi"
        role_3: "Quality Assurance"
        name_3: "Anna Neri"
        role_4: "Cliente"
        name_4: "Marco Blu"
        change_description: "Prima versione del documento"
---
# Test word template pages

Questo è un documento di test per verificare il funzionamento delle pagine predefinite nel sistema di export Word di MdExplorer.

## Architettura del Sistema

Il sistema XYZ è basato su un'architettura microservizi che include:

* Frontend Angular
* Backend .NET Core
* Database PostgreSQL
* Cache Redis

## Funzionalità Principali

### Modulo Utenti

Gestione completa degli utenti con:

* Autenticazione OAuth2
* Autorizzazione basata su ruoli
* Profili personalizzabili

### Modulo Reporting

Sistema di reporting avanzato con:

* Report dinamici
* Export in vari formati
* Schedulazione automatica

## Conclusioni

Il sistema XYZ rappresenta una soluzione completa per le esigenze aziendali moderne.
