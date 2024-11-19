import { useItems } from './api';
import { CreateChime, Loading, ChimeView } from './components';
import { Item } from './interfaces';
import { St8, St8View } from './St8';

export default function App() {
  const chimes = St8.map(useItems(), _ => _.filter(Item.isChime));

  return (
    <main className="space-y-8 max-w-md mx-auto">
      <CreateChime />

      <St8View
        data={chimes}
        loading={() => <Loading />}
        error={err => (
          <div className="bg-red-500 text-white p-4 rounded-lg shadow-md">
            <h2 className="font-bold text-lg">{err.message}: Unabled to load chimes</h2>
          </div>
        )}
      >
        {chimes => (
          <>
            {chimes.length > 0
              ? chimes.map(c => (
                <ChimeView key={c.id} chime={c} />
              ))
              : <p className="text-center">No Chimes :(</p>
            }
          </>
        )}
      </St8View>
    </main>
  );
}
