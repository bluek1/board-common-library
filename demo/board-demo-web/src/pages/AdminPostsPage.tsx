import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { postsApi } from '../api';
import { Post, PagedResult } from '../types';

export function AdminPostsPage() {
  const [posts, setPosts] = useState<Post[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedPosts, setSelectedPosts] = useState<number[]>([]);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');

  useEffect(() => {
    loadPosts();
  }, [page]);

  const loadPosts = async () => {
    setIsLoading(true);
    try {
      const data: PagedResult<Post> = await postsApi.getAll({ page, pageSize: 20 });
      setPosts(data.items);
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
    } catch (err) {
      setError('게시물을 불러오는데 실패했습니다.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSelectAll = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.checked) {
      setSelectedPosts(posts.map(p => p.id));
    } else {
      setSelectedPosts([]);
    }
  };

  const handleSelectPost = (postId: number) => {
    if (selectedPosts.includes(postId)) {
      setSelectedPosts(selectedPosts.filter(id => id !== postId));
    } else {
      setSelectedPosts([...selectedPosts, postId]);
    }
  };

  const handleDelete = async (postId: number) => {
    if (!window.confirm('이 게시물을 삭제하시겠습니까?')) return;

    try {
      await postsApi.delete(postId);
      setSuccessMessage('게시물이 삭제되었습니다.');
      loadPosts();
    } catch (err) {
      setError('게시물 삭제에 실패했습니다.');
    }
  };

  const handleBulkDelete = async () => {
    if (selectedPosts.length === 0) return;
    if (!window.confirm(`선택한 ${selectedPosts.length}개의 게시물을 삭제하시겠습니까?`)) return;

    try {
      await Promise.all(selectedPosts.map(id => postsApi.delete(id)));
      setSuccessMessage(`${selectedPosts.length}개의 게시물이 삭제되었습니다.`);
      setSelectedPosts([]);
      loadPosts();
    } catch (err) {
      setError('일부 게시물 삭제에 실패했습니다.');
    }
  };

  const handleTogglePin = async (post: Post) => {
    try {
      if (post.isPinned) {
        await postsApi.unpin(post.id);
      } else {
        await postsApi.pin(post.id);
      }
      loadPosts();
    } catch (err) {
      setError('상단 고정 변경에 실패했습니다.');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-gray-800">게시물 관리</h1>
        <Link
          to="/admin"
          className="text-blue-600 hover:underline"
        >
          ← 대시보드로 돌아가기
        </Link>
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 text-red-600 rounded-lg">
          {error}
          <button onClick={() => setError('')} className="ml-2 text-red-800">✕</button>
        </div>
      )}

      {successMessage && (
        <div className="p-4 bg-green-50 border border-green-200 text-green-600 rounded-lg">
          {successMessage}
          <button onClick={() => setSuccessMessage('')} className="ml-2 text-green-800">✕</button>
        </div>
      )}

      {/* 일괄 작업 */}
      {selectedPosts.length > 0 && (
        <div className="bg-blue-50 p-4 rounded-lg flex items-center justify-between">
          <span className="text-blue-600">{selectedPosts.length}개 선택됨</span>
          <button
            onClick={handleBulkDelete}
            className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
          >
            선택 삭제
          </button>
        </div>
      )}

      {/* 게시물 목록 */}
      <div className="bg-white rounded-xl shadow-lg overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50">
              <tr>
                <th className="py-3 px-4 text-left">
                  <input
                    type="checkbox"
                    checked={selectedPosts.length === posts.length && posts.length > 0}
                    onChange={handleSelectAll}
                    className="rounded"
                    aria-label="전체 선택"
                  />
                </th>
                <th className="py-3 px-4 text-left text-sm font-medium text-gray-500">ID</th>
                <th className="py-3 px-4 text-left text-sm font-medium text-gray-500">제목</th>
                <th className="py-3 px-4 text-left text-sm font-medium text-gray-500">작성자</th>
                <th className="py-3 px-4 text-left text-sm font-medium text-gray-500">조회</th>
                <th className="py-3 px-4 text-left text-sm font-medium text-gray-500">좋아요</th>
                <th className="py-3 px-4 text-left text-sm font-medium text-gray-500">댓글</th>
                <th className="py-3 px-4 text-left text-sm font-medium text-gray-500">상태</th>
                <th className="py-3 px-4 text-left text-sm font-medium text-gray-500">작성일</th>
                <th className="py-3 px-4 text-center text-sm font-medium text-gray-500">관리</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={10} className="py-8 text-center text-gray-500">
                    로딩 중...
                  </td>
                </tr>
              ) : posts.length === 0 ? (
                <tr>
                  <td colSpan={10} className="py-8 text-center text-gray-500">
                    게시물이 없습니다.
                  </td>
                </tr>
              ) : (
                posts.map((post) => (
                  <tr key={post.id} className="border-b border-gray-100 hover:bg-gray-50">
                    <td className="py-3 px-4">
                      <input
                        type="checkbox"
                        checked={selectedPosts.includes(post.id)}
                        onChange={() => handleSelectPost(post.id)}
                        className="rounded"
                        aria-label={`게시물 ${post.id} 선택`}
                      />
                    </td>
                    <td className="py-3 px-4 text-gray-600">{post.id}</td>
                    <td className="py-3 px-4">
                      <Link to={`/posts/${post.id}`} className="text-blue-600 hover:underline">
                        {post.isPinned && <span className="text-red-500 mr-1">📌</span>}
                        {post.title}
                      </Link>
                    </td>
                    <td className="py-3 px-4 text-gray-600">{post.authorName}</td>
                    <td className="py-3 px-4 text-gray-600">{post.viewCount}</td>
                    <td className="py-3 px-4 text-gray-600">{post.likeCount}</td>
                    <td className="py-3 px-4 text-gray-600">{post.commentCount}</td>
                    <td className="py-3 px-4">
                      <span className={`px-2 py-1 text-xs rounded-full ${
                        post.status === 1 ? 'bg-green-100 text-green-600' :
                        post.status === 0 ? 'bg-yellow-100 text-yellow-600' :
                        'bg-gray-100 text-gray-600'
                      }`}>
                        {post.status === 1 ? '게시됨' : post.status === 0 ? '임시저장' : '보관됨'}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-gray-500 text-sm">
                      {new Date(post.createdAt).toLocaleDateString('ko-KR')}
                    </td>
                    <td className="py-3 px-4">
                      <div className="flex items-center justify-center gap-2">
                        <button
                          onClick={() => handleTogglePin(post)}
                          className={`p-1 rounded ${post.isPinned ? 'text-red-500' : 'text-gray-400'} hover:bg-gray-100`}
                          title={post.isPinned ? '고정 해제' : '상단 고정'}
                        >
                          📌
                        </button>
                        <Link
                          to={`/posts/${post.id}/edit`}
                          className="p-1 text-blue-600 hover:bg-blue-50 rounded"
                          title="수정"
                        >
                          ✏️
                        </Link>
                        <button
                          onClick={() => handleDelete(post.id)}
                          className="p-1 text-red-600 hover:bg-red-50 rounded"
                          title="삭제"
                        >
                          🗑️
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* 페이지네이션 */}
        {totalPages > 1 && (
          <div className="px-4 py-3 border-t border-gray-200 flex items-center justify-between">
            <span className="text-sm text-gray-500">
              전체 {totalCount}개 중 {(page - 1) * 20 + 1}-{Math.min(page * 20, totalCount)}
            </span>
            <div className="flex gap-2">
              <button
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-3 py-1 border rounded disabled:opacity-50"
              >
                이전
              </button>
              <span className="px-3 py-1">
                {page} / {totalPages}
              </span>
              <button
                onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                className="px-3 py-1 border rounded disabled:opacity-50"
              >
                다음
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
