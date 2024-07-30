import { Link } from "@tanstack/react-router";

function handleLogin() {
  // TODO have a staff view
  alert("Not implemented");
}

export function LandingPage() {
  return (
    <div className="flex flex-row h-full place-content-evenly gap-4">
      <div
        className="grow card bg-primary text-primary-content shadow-lg hover:cursor-pointer group"
        onClick={handleLogin}
      >
        <div className="card-body mx-auto">
          <h2 className="card-title text-4xl group-hover:text-secondary">
            Login
          </h2>
        </div>
      </div>

      <div className="grow card card-bordered bg-accent text-primary-content group">
        <Link to="/reservations" preload="intent" className="block">
          <div className="card-body">
            <h2 className="card-title text-4xl mx-auto group-hover:text-secondary">
              Reserve
            </h2>
          </div>
        </Link>
      </div>
    </div>
  );
}
