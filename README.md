# PAYMENT-GATEWAY-INTEGRATION

COMPANY: CODTECH IT SOLUTIONS

NAME: JOELLE AMBAR

INTERN ID: CT08DN1843

DOMAIN: .NET WEB DEVELOPMENT

DURATION: 8 WEEKS

MENTOR: NEELA SANTHOSH

DESCRIPTION OF THE TASK

My core task was to "implement a payment gateway integration (e.g., PayPal or Stripe) into a .NET-based application," with the key "deliverable: secure and functional payment module." This involved building a complete payment processing flow from the user's perspective on the frontend to the backend's interaction with the payment gateway and its event handling.

My Implementation: Building the Secure and Functional Payment Module
My implementation focused on integrating Stripe as the chosen payment gateway within an ASP.NET Core web application. The process was structured around two main pillars: a client-side interface for payment collection and a robust server-side system for payment processing and event management.

On the frontend, I developed a "Product Checkout" page, which is accessible at https://localhost:7239/Payment/Checkout. This page clearly presents the item for purchase, "Super Cool Gadget," along with its price of $20.00. To ensure secure handling of sensitive payment information, I integrated Stripe.js, which provides specialized UI elements for collecting card details directly and securely from the user's browser. This client-side setup is designed to initiate a payment request to the backend without directly handling card data on my server, thereby adhering to security best practices.

The backend logic resides primarily within my PaymentController.cs. This controller hosts two critical action methods:

CreatePaymentIntent: This method is invoked by the frontend when a user initiates a payment. Its responsibility is to interact with the Stripe API via the Stripe .NET library to create a PaymentIntent. The PaymentIntent is a central Stripe object that tracks the lifecycle of a payment. Upon successful creation, this method returns the client_secret to the frontend, enabling Stripe.js to confirm the payment directly from the client's browser.
StripeWebhook: Recognizing the asynchronous nature of payment processing, I implemented a dedicated webhook endpoint. This [HttpPost] method, configured with the route "/stripe-webhook", is designed to receive real-time notifications (events) directly from Stripe regarding various payment states (e.g., payment succeeded, payment failed). This allows my application to react to the definitive outcome of a transaction, perform order fulfillment, or update database records without needing to continuously poll the Stripe API. The security of this endpoint relies on verifying the incoming Stripe signature against a secret configured in appsettings.json.
To facilitate development and testing, particularly for the webhook functionality, I utilized ngrok. This tool creates a secure public URL (e.g., https://9ca9-106-51-162-67.ngrok-free.app) that tunnels incoming requests to my local ASP.NET Core application running on https://localhost:7239. This allowed Stripe's webhook events to reach my development environment. Additionally, the Stripe CLI was employed to simulate various payment events (e.g., stripe trigger payment_intent.succeeded), which proved invaluable for testing the StripeWebhook endpoint's processing logic.

Through this comprehensive implementation, I've successfully established a secure and functional payment module within my .NET application, capable of handling both direct client-side payment initiation and asynchronous event-driven updates from the Stripe payment gateway.

OUTPUT

![Image](https://github.com/user-attachments/assets/3d6a62fe-f94e-4a27-b80b-ef9924e6d028)
![Image](https://github.com/user-attachments/assets/c4a3ad4f-5bf8-45c0-bd9f-330a0277be46)
![Image](https://github.com/user-attachments/assets/5b05c36f-d3ae-49f5-ac20-36cdef7bf679)

![Image](https://github.com/user-attachments/assets/7b5b38d1-f5bf-4853-93f9-f5ed117515e0)
![Image](https://github.com/user-attachments/assets/5344247c-a240-46ae-9d6d-165daae0f266)
