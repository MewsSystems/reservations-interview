import { Link, Outlet, useRouter } from "@tanstack/react-router";

export const Layout = () => {
  const { history } = useRouter();

  const hasHistory = window.history.length > 2; // ??

  function goBack() {
    history.go(-1);
  }

  return (
    <div>
      <div className="navbar bg-accent text-primary-content">
        <span
          title="Go Home"
          className="text-2xl underline underline-offset-4 hover:text-secondary"
        >
          <Link to="/">Reservations @ Mewstel</Link>
        </span>
        {hasHistory && (
          <button
            title="Go Back"
            className="ml-6 btn btn-ghost hover:text-secondary"
            onClick={goBack}
          >
            &lt;
          </button>
        )}
      </div>
      <div className="p-6">
        <Outlet />
      </div>
    </div>
  );
};
