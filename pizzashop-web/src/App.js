import React from 'react';
import Home from './pages/Home';
import About from './pages/About';
import Menu from './pages/Menu';
import StartOrder from './pages/StartOrder';
import Logout from './pages/Logout';
import Order from './pages/Order';
import NotFound from './pages/NotFound';
import Dashboard from './pages/Dashboard';
import NavMenu from './components/NavMenu';

function App() {
  const path = window.location.pathname.toLowerCase();

  let Page;
  switch (path) {
    case '/':
      Page = Home;
      break;
    case '/about':
      Page = About;
      break;
    case '/menu':
      Page = Menu;
      break;
    case '/start-order':
      Page = StartOrder;
      break;
    case '/logout':
      Page = Logout;
      break;
    case '/order':
      Page = Order;
      break;
    case '/dashboard':
      Page = Dashboard;
      break;
    default:
      Page = NotFound;
      break;
  }

  const hideNav = path === '/logout';
  const hideNavForStartOrder = path === '/start-order';

  return (
    <>
      {!hideNav && !hideNavForStartOrder && <NavMenu />}
      <div className="min-h-screen bg-[#7f2615] text-[#fef3c7]">
        <main className="max-w-6xl mx-auto py-10 md:py-12">
          <Page />
        </main>
      </div>
    </>
  );
}

export default App;
