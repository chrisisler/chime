import { useChimes } from './api';
import { CreateChime, Loading, ChimeView } from './components';
import { St8View } from './St8';

export default function App() {
  const chimes = useChimes();

  if (!Array.isArray(chimes)) {
    console.log(chimes);
  }

  return (
    <main className="space-y-8 max-w-md mx-auto ">
      <CreateChime />

      <St8View
        data={chimes}
        loading={() => <Loading />}
        error={err => (
          <div className="bg-red-500 text-white p-4 rounded-lg shadow-md">
            <h2 className="font-bold text-lg">Sorry, error loading data: {err.message}</h2>
          </div>
        )}
      >
        {chimes => (
          <>
            {chimes.map(c => (
              <ChimeView key={c.id} chime={c} />
            ))}
          </>
        )}
      </St8View>
    </main>
  );
}
