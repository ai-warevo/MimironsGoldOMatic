import { MimironsGoldOMaticPanelErrorBoundary } from './components/PanelErrorBoundary'
import { MimironsGoldOMaticViewerPanel } from './components/ViewerPanel'

export default function App() {
  return (
    <MimironsGoldOMaticPanelErrorBoundary>
      <MimironsGoldOMaticViewerPanel />
    </MimironsGoldOMaticPanelErrorBoundary>
  )
}
