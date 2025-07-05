# Specification

# 1. Sequence Design Overview

- **Sequence_Diagram:**
  ### . User Authentication via External Identity Provider (OIDC)
  Shows the OIDC/OAuth 2.0 redirect flow for user authentication. The user is redirected to an external IdP (e.g., Azure AD, Keycloak), authenticates there, and is redirected back to the application with an authorization code, which is then exchanged for a token.

  #### .4. Purpose
  To document the integration with an external Identity Provider, which is a critical security and enterprise integration feature.

  #### .5. Type
  AuthenticationFlow

  #### .6. Participant Repository Ids
  
  - REPO-APP-PRESENTATION
  - REPO-APP-AUTH
  - ExternalIdP
  
  #### .7. Key Interactions
  
  - User clicks 'Login with IdP' button on the Blazor UI (Presentation Layer).
  - The request is forwarded to the Authentication Service, which crafts an OIDC authentication request.
  - User's browser is redirected to the external IdP's login page.
  - User authenticates with the external IdP.
  - IdP redirects the user back to the application's callback URL with an authorization code.
  - Authentication Service receives the code, contacts the IdP's token endpoint to exchange the code for an ID token and access token.
  - Authentication Service validates the token, creates a local session/JWT, and maps IdP roles to internal system roles.
  - User is granted access to the web application.
  
  #### .8. Related Feature Ids
  
  - REQ-3-004
  - REQ-3-016
  
  #### .9. Domain
  Security

  #### .10. Metadata
  
  - **Complexity:** High
  - **Priority:** Critical
  


---

# 2. Sequence Diagram Details

- **Success:** True
- **Cache_Created:** True
- **Status:** refreshed
- **Cache_Id:** kswlxre8o74szkiwvuu0e4l7r0ecbo3ndfhznnp8
- **Cache_Name:** cachedContents/kswlxre8o74szkiwvuu0e4l7r0ecbo3ndfhznnp8
- **Cache_Display_Name:** repositories
- **Cache_Status_Verified:** True
- **Model:** models/gemini-2.5-pro-preview-03-25
- **Workflow_Id:** I9v2neJ0O4zJsz8J
- **Execution_Id:** AIzaSyCGei_oYXMpZW-N3d-yH-RgHKXz8dsixhc
- **Project_Id:** 10
- **Record_Id:** 12
- **Cache_Type:** repositories


---

